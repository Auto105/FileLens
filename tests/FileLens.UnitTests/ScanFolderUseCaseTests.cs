using FileLens.Application.DependencyInjection;
using FileLens.Application.DTO;
using FileLens.Application.Interfaces;
using FileLens.Application.Services;
using FileLens.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileLens.UnitTests;

[TestClass]
public sealed class ScanFolderUseCaseTests
{
    [TestMethod]
    [DataRow("C:\\requested\\folder")]
    [DataRow("relative/../requested")]
    [DataRow(" requested folder ")]
    public async Task ValidInputForwardsExactPathAndTokenOnce(string path)
    {
        var expected = CreateResult(ScanCompletionStatus.Complete);
        var scanner = new FakeFolderScanner((_, _) => Task.FromResult(expected));
        using var cancellation = new CancellationTokenSource();
        var useCase = new ScanFolderUseCase(scanner);

        var actual = await useCase.ExecuteAsync(path, cancellation.Token);

        Assert.AreEqual(1, scanner.InvocationCount);
        Assert.AreEqual(path, scanner.ReceivedFolderPath);
        Assert.AreEqual(cancellation.Token, scanner.ReceivedCancellationToken);
        Assert.AreSame(expected, actual);
    }

    [TestMethod]
    [DataRow(ScanCompletionStatus.Complete)]
    [DataRow(ScanCompletionStatus.Partial)]
    public async Task ResultIncludingDiagnosticsIsReturnedAsOriginalInstance(ScanCompletionStatus status)
    {
        var expected = CreateResult(status);
        var scanner = new FakeFolderScanner((_, _) => Task.FromResult(expected));

        var actual = await new ScanFolderUseCase(scanner).ExecuteAsync("requested");

        Assert.AreSame(expected, actual);
        Assert.AreSame(expected.RootFolder, actual.RootFolder);
        Assert.AreSame(expected.Diagnostics, actual.Diagnostics);
        Assert.AreEqual(status, actual.CompletionStatus);
        Assert.AreEqual(default, scanner.ReceivedCancellationToken);
    }

    [TestMethod]
    public async Task NullInputFaultsTaskWithoutCallingScanner()
    {
        var scanner = CreateSuccessfulScanner();
        var task = new ScanFolderUseCase(scanner).ExecuteAsync(null!);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => task);

        Assert.AreEqual("folderPath", exception.ParamName);
        Assert.IsTrue(task.IsFaulted);
        Assert.AreEqual(0, scanner.InvocationCount);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("\t\r\n")]
    public async Task BlankInputFaultsTaskWithoutCallingScanner(string path)
    {
        var scanner = CreateSuccessfulScanner();
        var task = new ScanFolderUseCase(scanner).ExecuteAsync(path);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => task);

        Assert.AreEqual("folderPath", exception.ParamName);
        Assert.IsTrue(task.IsFaulted);
        Assert.AreEqual(0, scanner.InvocationCount);
    }

    [TestMethod]
    [DataRow("requested")]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("\t ")]
    public async Task PreCanceledTokenTakesPriorityAndDoesNotCallScanner(string? path)
    {
        var scanner = CreateSuccessfulScanner();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var task = new ScanFolderUseCase(scanner).ExecuteAsync(path!, cancellation.Token);

        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => task);

        Assert.AreEqual(cancellation.Token, exception.CancellationToken);
        Assert.IsTrue(task.IsCanceled);
        Assert.AreEqual(0, scanner.InvocationCount);
    }

    [TestMethod]
    public async Task RunningCancellationPropagatesWithoutResult()
    {
        var completion = new TaskCompletionSource<ScanResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var scanner = new FakeFolderScanner((_, _) => completion.Task);
        using var cancellation = new CancellationTokenSource();
        var task = new ScanFolderUseCase(scanner).ExecuteAsync("requested", cancellation.Token);
        Assert.AreEqual(1, scanner.InvocationCount);
        Assert.AreEqual(cancellation.Token, scanner.ReceivedCancellationToken);
        Assert.IsFalse(task.IsCompleted);

        using var registration = scanner.ReceivedCancellationToken.Register(
            () => completion.TrySetCanceled(scanner.ReceivedCancellationToken));
        cancellation.Cancel();
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(
            () => task.WaitAsync(TimeSpan.FromSeconds(10)));

        Assert.AreEqual(cancellation.Token, exception.CancellationToken);
        Assert.IsTrue(task.IsCanceled);
        Assert.AreEqual(1, scanner.InvocationCount);
    }

    [TestMethod]
    public async Task ScannerCancellationExceptionIsNotWrapped()
    {
        using var cancellation = new CancellationTokenSource();
        var expected = new OperationCanceledException(cancellation.Token);
        var scanner = new FakeFolderScanner((_, _) => Task.FromException<ScanResult>(expected));
        var task = new ScanFolderUseCase(scanner).ExecuteAsync("requested", cancellation.Token);

        var actual = await Assert.ThrowsAsync<OperationCanceledException>(() => task);

        Assert.AreSame(expected, actual);
        Assert.IsTrue(task.IsCanceled);
    }

    [TestMethod]
    [DataRow(typeof(DirectoryNotFoundException), false)]
    [DataRow(typeof(DirectoryNotFoundException), true)]
    [DataRow(typeof(UnauthorizedAccessException), false)]
    [DataRow(typeof(UnauthorizedAccessException), true)]
    [DataRow(typeof(NotSupportedException), false)]
    [DataRow(typeof(NotSupportedException), true)]
    [DataRow(typeof(IOException), false)]
    [DataRow(typeof(IOException), true)]
    public async Task ScannerFailuresPropagateSameExceptionFromThrowOrFault(Type type, bool synchronousThrow)
    {
        var expected = (Exception)Activator.CreateInstance(type, "Scanner failure.")!;
        var scanner = new FakeFolderScanner((_, _) => synchronousThrow
            ? throw expected
            : Task.FromException<ScanResult>(expected));
        var task = new ScanFolderUseCase(scanner).ExecuteAsync("requested");

        var actual = await Assert.ThrowsAsync<Exception>(() => task);

        Assert.AreSame(expected, actual);
        Assert.IsTrue(task.IsFaulted);
        Assert.AreEqual(1, scanner.InvocationCount);
    }

    [TestMethod]
    public async Task ScannerResultIsNotReplacedByAdditionalCancellationCheck()
    {
        using var cancellation = new CancellationTokenSource();
        var expected = CreateResult(ScanCompletionStatus.Complete);
        var scanner = new FakeFolderScanner((_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(expected);
        });

        var actual = await new ScanFolderUseCase(scanner).ExecuteAsync("requested", cancellation.Token);

        Assert.AreSame(expected, actual);
    }

    [TestMethod]
    public async Task ApplicationRegistrationResolvesTransientUseCasesWithInjectedScanner()
    {
        var scanner = CreateSuccessfulScanner();
        var services = new ServiceCollection();
        services.AddApplicationServices();
        services.AddSingleton<IFolderScanner>(scanner);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        var first = provider.GetRequiredService<IScanFolderUseCase>();
        var second = provider.GetRequiredService<IScanFolderUseCase>();

        Assert.IsInstanceOfType<ScanFolderUseCase>(first);
        Assert.IsInstanceOfType<ScanFolderUseCase>(second);
        Assert.AreNotSame(first, second);
        await first.ExecuteAsync("first");
        await second.ExecuteAsync("second");
        Assert.AreEqual(2, scanner.InvocationCount);
        Assert.AreEqual("second", scanner.ReceivedFolderPath);
    }

    [TestMethod]
    public void ConstructorRejectsNullScanner()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ScanFolderUseCase(null!));
        Assert.AreEqual("scanner", exception.ParamName);
    }

    private static FakeFolderScanner CreateSuccessfulScanner() =>
        new((_, _) => Task.FromResult(CreateResult(ScanCompletionStatus.Complete)));

    private static ScanResult CreateResult(ScanCompletionStatus status)
    {
        var root = new FolderNode("result-root", [], []);
        var partial = status == ScanCompletionStatus.Partial;
        ScanDiagnostic[] diagnostics =
        [
            new("excluded", ScanDiagnosticKind.Excluded, ScanDiagnosticCategory.ReparsePointExcluded),
            .. partial
                ? new[] { new ScanDiagnostic("missing", ScanDiagnosticKind.Failure, ScanDiagnosticCategory.NotFound) }
                : Array.Empty<ScanDiagnostic>()
        ];
        return new ScanResult(root, 1, 0, 0, status, partial ? 1 : 0, 1, diagnostics);
    }
}
