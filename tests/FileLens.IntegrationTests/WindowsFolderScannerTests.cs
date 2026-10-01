using FileLens.Application.DTO;
using FileLens.Application.Interfaces;
using FileLens.Infrastructure.Filesystem;
using FileLens.IntegrationTests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: DoNotParallelize]

namespace FileLens.IntegrationTests;

[TestClass]
public sealed class WindowsFolderScannerTests
{
    [TestMethod]
    public async Task EmptyRootIncludesOnlyTheRoot()
    {
        using var folder = new TemporaryFolder();
        IFolderScanner scanner = new WindowsFolderScanner();
        var result = await scanner.ScanAsync(folder.RootPath);
        Assert.AreEqual(1, result.TotalFolderCount);
        Assert.AreEqual(0, result.TotalFileCount);
        Assert.AreEqual(0L, result.TotalSize);
        Assert.AreEqual(ScanCompletionStatus.Complete, result.CompletionStatus);
        Assert.AreEqual(0L, result.FailureCount);
        Assert.AreEqual(0L, result.ExcludedEntryCount);
        Assert.AreEqual(0, result.Diagnostics.Count);
    }

    [TestMethod]
    public async Task NestedTreeAndLogicalSummaryMatchRetainedNodes()
    {
        using var folder = new TemporaryFolder();
        folder.CreateDirectory("A/B");
        folder.CreateDirectory("empty");
        folder.CreateFile("zero.bin");
        folder.CreateFile("A/three.bin", 3);
        folder.CreateFile("A/B/seven.bin", 7);
        folder.CreateFile("large.bin", 1024);
        var result = await new WindowsFolderScanner().ScanAsync(folder.RootPath);
        Assert.AreEqual(4, result.TotalFolderCount);
        Assert.AreEqual(4, result.TotalFileCount);
        Assert.AreEqual(1034L, result.TotalSize);
        Assert.AreEqual(0, result.RootFolder.ChildFolders.Single(node => Path.GetFileName(node.FolderPath) == "empty").Files.Count);
        Assert.AreEqual(0L, result.RootFolder.Files.Single(node => Path.GetFileName(node.FilePath) == "zero.bin").FileSize);
    }

    [TestMethod]
    public async Task FileMetadataMatchesObservedFilesystemValues()
    {
        using var folder = new TemporaryFolder();
        var path = folder.CreateFile("metadata.bin", 17);
        File.SetCreationTimeUtc(path, new DateTime(2024, 1, 2, 3, 4, 6, DateTimeKind.Utc));
        File.SetLastWriteTimeUtc(path, new DateTime(2024, 2, 3, 4, 5, 8, DateTimeKind.Utc));
        var expected = new FileInfo(path);
        var result = await new WindowsFolderScanner().ScanAsync(folder.RootPath);
        var file = result.RootFolder.Files.Single();
        Assert.AreEqual(path, file.FilePath);
        Assert.AreEqual(".bin", file.FileExtension);
        Assert.AreEqual(17L, file.FileSize);
        Assert.AreEqual(expected.CreationTimeUtc, file.CreationTimeUtc);
        Assert.AreEqual(expected.LastWriteTimeUtc, file.LastWriteTimeUtc);
        Assert.AreEqual(expected.Attributes, file.Attributes);
    }

    [TestMethod]
    public async Task RelativeInputAndDotSegmentsProduceAbsoluteDtoPaths()
    {
        using var folder = new TemporaryFolder();
        folder.CreateDirectory("nested");
        folder.CreateFile("nested/file.bin", 2);
        var relative = Path.GetRelativePath(Environment.CurrentDirectory, folder.RootPath);
        var result = await new WindowsFolderScanner().ScanAsync(Path.Combine(relative, "nested", "..", "."));
        Assert.AreEqual(folder.RootPath, result.RootFolder.FolderPath);
        var child = result.RootFolder.ChildFolders.Single();
        Assert.AreEqual(folder.PathOf("nested"), child.FolderPath);
        Assert.AreEqual(folder.PathOf("nested/file.bin"), child.Files.Single().FilePath);
        Assert.IsTrue(Path.IsPathFullyQualified(child.Files.Single().FilePath));
    }

    [TestMethod]
    public async Task TrailingSeparatorDoesNotChangeResult()
    {
        using var folder = new TemporaryFolder();
        folder.CreateFile("file.bin", 3);
        var scanner = new WindowsFolderScanner();
        var plain = await scanner.ScanAsync(folder.RootPath);
        var trailing = await scanner.ScanAsync(folder.RootPath + Path.DirectorySeparatorChar);
        Assert.AreEqual(plain.RootFolder.FolderPath, trailing.RootFolder.FolderPath);
        Assert.AreEqual(plain.TotalFolderCount, trailing.TotalFolderCount);
        Assert.AreEqual(plain.TotalFileCount, trailing.TotalFileCount);
        Assert.AreEqual(plain.TotalSize, trailing.TotalSize);
    }

    [TestMethod]
    public async Task MissingRootFaultsWithoutResult()
    {
        using var folder = new TemporaryFolder();
        await Assert.ThrowsExactlyAsync<DirectoryNotFoundException>(
            () => new WindowsFolderScanner().ScanAsync(folder.PathOf("missing")));
    }

    [TestMethod]
    public async Task FileCannotBeUsedAsRoot()
    {
        using var folder = new TemporaryFolder();
        var path = folder.CreateFile("file.bin");
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => new WindowsFolderScanner().ScanAsync(path));
    }

    [TestMethod]
    public async Task NullAndBlankInputsAreRejected()
    {
        var scanner = new WindowsFolderScanner();
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => scanner.ScanAsync(null!));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => scanner.ScanAsync(""));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => scanner.ScanAsync(" \t "));
    }

    [TestMethod]
    public async Task UnsupportedPathFormsAreRejectedBeforeTraversal()
    {
        var scanner = new WindowsFolderScanner();
        foreach (var path in new[] { @"\\server\share", @"\\?\C:\", @"\\.\C:\", @"C:folder", @"\folder" })
        {
            await Assert.ThrowsExactlyAsync<NotSupportedException>(() => scanner.ScanAsync(path));
        }
    }

    [TestMethod]
    public async Task HiddenAndSystemAttributesAloneDoNotExcludeEntries()
    {
        using var folder = new TemporaryFolder();
        var directory = folder.CreateDirectory("hidden");
        var path = folder.CreateFile("hidden/file.bin", 5);
        File.SetAttributes(directory, File.GetAttributes(directory) | FileAttributes.Hidden | FileAttributes.System);
        File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden | FileAttributes.System);
        var result = await new WindowsFolderScanner().ScanAsync(folder.RootPath);
        Assert.AreEqual(2, result.TotalFolderCount);
        Assert.AreEqual(1, result.TotalFileCount);
        Assert.AreEqual(5L, result.TotalSize);
        Assert.AreEqual(0L, result.ExcludedEntryCount);
    }

    [TestMethod]
    public void FixtureRejectsOwnershipEscape()
    {
        using var folder = new TemporaryFolder();
        Assert.ThrowsExactly<IOException>(() => folder.PathOf("../../escape"));
        Assert.ThrowsExactly<IOException>(() => folder.OwnedPath("../escape"));
    }

    [TestMethod]
    [TestCategory("EnvironmentDependent")]
    public async Task ExistingMappedNetworkDriveIsUnsupported()
    {
        var drive = DriveInfo.GetDrives().FirstOrDefault(item => item.DriveType == DriveType.Network);
        if (drive is null)
        {
            Assert.Inconclusive("No existing mapped network drive is available; the test does not create a mapping.");
        }

        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => new WindowsFolderScanner().ScanAsync(drive!.Name));
    }

    [TestMethod]
    [TestCategory("EnvironmentDependent")]
    public async Task ActualWindowsDirectoryRootIsRejectedReadOnly()
    {
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (string.IsNullOrEmpty(windows))
        {
            Assert.Inconclusive("The Windows installation directory is unavailable.");
        }

        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => new WindowsFolderScanner().ScanAsync(windows));
    }
}
