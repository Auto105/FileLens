using FileLens.Application.DTO;
using FileLens.Infrastructure.Filesystem;
using FileLens.IntegrationTests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileLens.IntegrationTests;

[TestClass]
public sealed class WindowsFolderScannerReparseTests
{
    [TestMethod]
    [TestCategory("EnvironmentDependent")]
    public async Task FileSymbolicLinkIsExcludedWithoutCountingTarget()
    {
        using var folder = new TemporaryFolder();
        var target = folder.OwnedPath("target.bin");
        File.WriteAllBytes(target, new byte[19]);
        TestEnvironment.CreateSymbolicLink(folder.PathOf("link.bin"), target, false);
        var result = await new WindowsFolderScanner().ScanAsync(folder.RootPath);
        AssertExcludedOnly(result);
        Assert.AreEqual(0L, result.TotalSize);
    }

    [TestMethod]
    [TestCategory("EnvironmentDependent")]
    public async Task DirectorySymbolicLinkDoesNotTraverseOutsideRoot()
    {
        using var folder = new TemporaryFolder();
        var target = folder.OwnedPath("target");
        Directory.CreateDirectory(target);
        File.WriteAllBytes(Path.Combine(target, "outside.bin"), new byte[11]);
        TestEnvironment.CreateSymbolicLink(folder.PathOf("link"), target, true);
        var result = await new WindowsFolderScanner().ScanAsync(folder.RootPath);
        AssertExcludedOnly(result);
    }

    [TestMethod]
    [TestCategory("EnvironmentDependent")]
    public async Task JunctionCycleIsExcluded()
    {
        using var folder = new TemporaryFolder();
        TestEnvironment.CreateJunction(folder.PathOf("cycle"), folder.RootPath);
        var result = await new WindowsFolderScanner().ScanAsync(folder.RootPath).WaitAsync(TimeSpan.FromSeconds(10));
        AssertExcludedOnly(result);
    }

    [TestMethod]
    [TestCategory("EnvironmentDependent")]
    public async Task JunctionOutsideRootIsExcludedAndCleanupPreservesItsTarget()
    {
        using var target = new TemporaryFolder();
        var targetFile = target.CreateFile("outside.bin", 23);
        using (var folder = new TemporaryFolder())
        {
            TestEnvironment.CreateJunction(folder.PathOf("link"), target.RootPath);
            var result = await new WindowsFolderScanner().ScanAsync(folder.RootPath);
            AssertExcludedOnly(result);
        }

        Assert.IsTrue(File.Exists(targetFile), "Cleanup must delete the junction itself, not its target.");
        Assert.AreEqual(23L, new FileInfo(targetFile).Length);
    }

    [TestMethod]
    [TestCategory("EnvironmentDependent")]
    public async Task ReparseRootAndReparseAncestorAreUnsupported()
    {
        using var folder = new TemporaryFolder();
        var target = folder.OwnedPath("target");
        Directory.CreateDirectory(Path.Combine(target, "child"));
        var link = folder.PathOf("link");
        TestEnvironment.CreateJunction(link, target);
        var scanner = new WindowsFolderScanner();
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => scanner.ScanAsync(link));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => scanner.ScanAsync(Path.Combine(link, "child")));
    }

    private static void AssertExcludedOnly(ScanResult result)
    {
        Assert.AreEqual(ScanCompletionStatus.Complete, result.CompletionStatus);
        Assert.AreEqual(0L, result.FailureCount);
        Assert.AreEqual(1L, result.ExcludedEntryCount);
        Assert.AreEqual(1, result.TotalFolderCount);
        Assert.AreEqual(0, result.TotalFileCount);
        var diagnostic = result.Diagnostics.Single();
        Assert.AreEqual(ScanDiagnosticKind.Excluded, diagnostic.Kind);
        Assert.AreEqual(ScanDiagnosticCategory.ReparsePointExcluded, diagnostic.Category);
    }
}
