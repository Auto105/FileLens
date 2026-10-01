using FileLens.Application.DTO;
using FileLens.Infrastructure.Filesystem;
using FileLens.IntegrationTests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileLens.IntegrationTests;

[TestClass]
public sealed class WindowsFolderScannerDiagnosticsTests
{
    // Regression against the current internal MVP default; not a permanent public contract.
    private const int CurrentDetailLimit = 1_000;
    private const int EntryCountBeyondLimit = CurrentDetailLimit + 5;

    [TestMethod]
    public async Task ObservedFileDeletionProducesPartialNotFoundAndKeepsSibling()
    {
        using var folder = new TemporaryFolder();
        var missing = folder.CreateFile("removed.bin", 7);
        var retained = folder.CreateFile("retained.bin", 3);
        var scanner = new WindowsFolderScanner(path =>
        {
            if (path == missing)
            {
                File.Delete(path);
            }
        });
        var result = await scanner.ScanAsync(folder.RootPath);
        Assert.AreEqual(ScanCompletionStatus.Partial, result.CompletionStatus);
        Assert.AreEqual(1L, result.FailureCount);
        Assert.AreEqual(0L, result.ExcludedEntryCount);
        Assert.AreEqual(1, result.TotalFileCount);
        Assert.AreEqual(3L, result.TotalSize);
        Assert.AreEqual(retained, result.RootFolder.Files.Single().FilePath);
        var diagnostic = result.Diagnostics.Single();
        Assert.AreEqual(missing, diagnostic.Path);
        Assert.AreEqual(ScanDiagnosticKind.Failure, diagnostic.Kind);
        Assert.AreEqual(ScanDiagnosticCategory.NotFound, diagnostic.Category);
        Assert.IsNotNull(diagnostic.ExceptionType);
        Assert.IsNotNull(diagnostic.ErrorCode);
    }

    [TestMethod]
    public async Task ObservedRootDisappearanceFaultsWithoutReturningPartialResult()
    {
        using var folder = new TemporaryFolder();
        folder.CreateFile("observed.bin", 1);
        var moved = false;
        var scanner = new WindowsFolderScanner(_ =>
        {
            if (!moved)
            {
                Directory.Move(folder.RootPath, folder.OwnedPath("moved"));
                moved = true;
            }
        });
        await Assert.ThrowsExactlyAsync<DirectoryNotFoundException>(() => scanner.ScanAsync(folder.RootPath));
        Assert.IsTrue(moved, "The scan must reach the checkpoint before root disappearance.");
    }

    [TestMethod]
    public async Task FailureCounterContinuesAfterDiagnosticDetailsStop()
    {
        using var folder = new TemporaryFolder();
        for (var index = 0; index < EntryCountBeyondLimit; index++)
        {
            folder.CreateFile($"file-{index:D4}.bin");
        }

        var scanner = new WindowsFolderScanner(File.Delete);
        var result = await scanner.ScanAsync(folder.RootPath);
        Assert.AreEqual(ScanCompletionStatus.Partial, result.CompletionStatus);
        Assert.AreEqual((long)EntryCountBeyondLimit, result.FailureCount);
        Assert.AreEqual(0L, result.ExcludedEntryCount);
        Assert.AreEqual(CurrentDetailLimit, result.Diagnostics.Count);
        Assert.AreEqual(0, result.TotalFileCount);
        Assert.IsTrue(result.Diagnostics.All(item => item.Kind == ScanDiagnosticKind.Failure));
        Assert.IsTrue(result.Diagnostics.Count < result.FailureCount + result.ExcludedEntryCount);
    }

    [TestMethod]
    [TestCategory("EnvironmentDependent")]
    public async Task ExclusionCounterContinuesAfterDiagnosticDetailsStop()
    {
        using var folder = new TemporaryFolder();
        var target = folder.OwnedPath("target");
        Directory.CreateDirectory(target);
        for (var index = 0; index < EntryCountBeyondLimit; index++)
        {
            TestEnvironment.CreateJunction(folder.PathOf($"link-{index:D4}"), target);
        }

        var result = await new WindowsFolderScanner().ScanAsync(folder.RootPath);
        Assert.AreEqual(ScanCompletionStatus.Complete, result.CompletionStatus);
        Assert.AreEqual(0L, result.FailureCount);
        Assert.AreEqual((long)EntryCountBeyondLimit, result.ExcludedEntryCount);
        Assert.AreEqual(CurrentDetailLimit, result.Diagnostics.Count);
        Assert.AreEqual(1, result.TotalFolderCount);
        Assert.IsTrue(result.Diagnostics.All(item =>
            item.Kind == ScanDiagnosticKind.Excluded && item.Category == ScanDiagnosticCategory.ReparsePointExcluded));
    }

    [TestMethod]
    [TestCategory("EnvironmentDependent")]
    public async Task FailuresAndPolicyExclusionsRemainDistinct()
    {
        using var folder = new TemporaryFolder();
        var target = folder.OwnedPath("target");
        Directory.CreateDirectory(target);
        TestEnvironment.CreateJunction(folder.PathOf("link"), target);
        var removed = folder.CreateFile("removed.bin", 1);
        var scanner = new WindowsFolderScanner(path =>
        {
            if (path == removed)
            {
                File.Delete(path);
            }
        });
        var result = await scanner.ScanAsync(folder.RootPath);
        Assert.AreEqual(ScanCompletionStatus.Partial, result.CompletionStatus);
        Assert.AreEqual(1L, result.FailureCount);
        Assert.AreEqual(1L, result.ExcludedEntryCount);
        var failure = result.Diagnostics.Single(item => item.Kind == ScanDiagnosticKind.Failure);
        var exclusion = result.Diagnostics.Single(item => item.Kind == ScanDiagnosticKind.Excluded);
        Assert.AreEqual(ScanDiagnosticCategory.NotFound, failure.Category);
        Assert.AreEqual(ScanDiagnosticCategory.ReparsePointExcluded, exclusion.Category);
        Assert.IsNull(exclusion.ExceptionType);
        Assert.IsNull(exclusion.ErrorCode);
    }

    [TestMethod]
    public async Task ConcurrentScansDoNotShareFailureState()
    {
        using var failing = new TemporaryFolder();
        using var normal = new TemporaryFolder();
        var removed = failing.CreateFile("removed.bin");
        normal.CreateFile("retained.bin", 2);
        var scanner = new WindowsFolderScanner(path =>
        {
            if (path == removed)
            {
                File.Delete(path);
            }
        });
        var results = await Task.WhenAll(scanner.ScanAsync(failing.RootPath), scanner.ScanAsync(normal.RootPath));
        Assert.AreEqual(ScanCompletionStatus.Partial, results[0].CompletionStatus);
        Assert.AreEqual(1L, results[0].FailureCount);
        Assert.AreEqual(ScanCompletionStatus.Complete, results[1].CompletionStatus);
        Assert.AreEqual(0L, results[1].FailureCount);
        Assert.AreEqual(0, results[1].Diagnostics.Count);
        Assert.AreEqual(2L, results[1].TotalSize);
    }
}
