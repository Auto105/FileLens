using FileLens.Infrastructure.Filesystem;
using FileLens.IntegrationTests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileLens.IntegrationTests;

[TestClass]
public sealed class WindowsFolderScannerCancellationTests
{
    [TestMethod]
    public async Task PreCanceledTokenReturnsCancellationWithoutResult()
    {
        using var folder = new TemporaryFolder();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var scan = new WindowsFolderScanner().ScanAsync(folder.RootPath, cancellation.Token);
        await Assert.ThrowsAsync<OperationCanceledException>(() => scan);
        Assert.IsTrue(scan.IsCanceled);
    }

    [TestMethod]
    public async Task RunningCancellationIsSynchronizedWithAnObservedEntry()
    {
        using var folder = new TemporaryFolder();
        folder.CreateFile("observed.bin", 1);
        using var cancellation = new CancellationTokenSource();
        using var release = new ManualResetEventSlim();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var scanner = new WindowsFolderScanner(_ =>
        {
            started.TrySetResult();
            if (!release.Wait(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException("The test did not release the scanner checkpoint.");
            }
        });
        var scan = scanner.ScanAsync(folder.RootPath, cancellation.Token);
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsFalse(scan.IsCompleted, "The worker must be inside the checkpoint.");
            cancellation.Cancel();
            release.Set();
            await Assert.ThrowsAsync<OperationCanceledException>(() => scan.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(scan.IsCanceled);
        }
        finally
        {
            release.Set();
            try
            {
                await scan.WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (OperationCanceledException)
            {
                // Expected termination, awaited before fixture cleanup.
            }
        }
    }
}
