using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows.Threading;
using FileLens.Application.Interfaces;
using FileLens.Bootstrap.Hosting;
using FileLens.Infrastructure.Configuration;
using FileLens.Infrastructure.Filesystem;
using FileLens.UI.ViewModels;
using FileLens.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;

[assembly: DoNotParallelize]

namespace FileLens.BootstrapTests;

[TestClass]
public sealed class HostCompositionTests
{
    [TestMethod]
    public void ProductionHostBuildsWithValidatedRegistrations()
    {
        using var host = BuildHost(CreateLogDirectory());
        Assert.IsNotNull(host.Services.GetRequiredService<IHostApplicationLifetime>());
    }

    [TestMethod]
    public void ScannerResolvesAsWindowsScannerWithTransientLifetime()
    {
        using var host = BuildHost(CreateLogDirectory());
        var first = host.Services.GetRequiredService<IFolderScanner>();
        var second = host.Services.GetRequiredService<IFolderScanner>();

        Assert.IsInstanceOfType<WindowsFolderScanner>(first);
        Assert.IsInstanceOfType<WindowsFolderScanner>(second);
        Assert.AreNotSame(first, second);
    }

    [TestMethod]
    public void ShellViewModelResolvesWithTransientLifetime()
    {
        using var host = BuildHost(CreateLogDirectory());
        var first = host.Services.GetRequiredService<MainWindowViewModel>();
        var second = host.Services.GetRequiredService<MainWindowViewModel>();

        Assert.IsNotNull(first);
        Assert.AreNotSame(first, second);
    }

    [TestMethod]
    public async Task DefaultHostLifetimeStartsAndStopsAndReleasesLogFile()
    {
        var directory = CreateLogDirectory();
        var staticLogger = Log.Logger;
        using (var host = BuildHost(directory))
        {
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await host.StartAsync(timeout.Token);
            Assert.IsTrue(lifetime.ApplicationStarted.IsCancellationRequested);
            host.Services.GetRequiredService<ILogger<HostCompositionTests>>()
                .LogInformation("Bootstrap lifecycle verification marker.");

            await host.StopAsync(timeout.Token);
            Assert.IsTrue(lifetime.ApplicationStopped.IsCancellationRequested);
            Assert.AreSame(staticLogger, Log.Logger);
        }

        var files = Directory.GetFiles(directory, "filelens-*.log");
        Assert.HasCount(1, files);
        using var stream = new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.None);
        using var reader = new StreamReader(stream);
        StringAssert.Contains(await reader.ReadToEndAsync(), "Bootstrap lifecycle verification marker.");
    }

    [TestMethod]
    public void MainWindowReceivesItsViewModelOnStaWithoutCreatingApplication()
    {
        using var host = BuildHost(CreateLogDirectory());
        ExceptionDispatchInfo? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                Assert.IsNull(System.Windows.Application.Current);
                var first = host.Services.GetRequiredService<MainWindow>();
                var second = host.Services.GetRequiredService<MainWindow>();
                Assert.IsInstanceOfType<MainWindowViewModel>(first.DataContext);
                Assert.IsInstanceOfType<MainWindowViewModel>(second.DataContext);
                Assert.AreNotSame(first, second);
                Assert.AreNotSame(first.DataContext, second.DataContext);
                Assert.AreEqual("FileLens", first.Title);
                Assert.IsNotNull(first.Content);
            }
            catch (Exception exception)
            {
                failure = ExceptionDispatchInfo.Capture(exception);
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(30)), "STA window construction timed out.");
        failure?.Throw();
    }

    [TestMethod]
    public void DefaultRelativeLogDirectoryUsesLocalApplicationData()
    {
        var builder = BootstrapHostBuilder.Create();
        // Isolate the default from environment variables supplied by the test runner.
        builder.Configuration["FileLens:Logging:LogDirectory"] = "logs";
        // Resolve options without building Host logging or writing into the user's log directory.
        using var services = builder.Services.BuildServiceProvider();
        var options = services.GetRequiredService<IOptions<FileLensLoggingOptions>>().Value;

        Assert.AreEqual(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FileLens", "logs"), options.LogDirectory);
    }

    [TestMethod]
    public void ExplicitAbsoluteLogDirectoryIsPreserved()
    {
        var directory = CreateLogDirectory();
        using var host = BuildHost(directory);
        Assert.AreEqual(directory,
            host.Services.GetRequiredService<IOptions<FileLensLoggingOptions>>().Value.LogDirectory);
    }

    private static IHost BuildHost(string directory)
    {
        var builder = BootstrapHostBuilder.Create(
            [$"--FileLens:Logging:LogDirectory={directory}"]);
        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        }));
        return builder.Build();
    }

    private static string CreateLogDirectory()
    {
        var repository = new DirectoryInfo(AppContext.BaseDirectory);
        while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "FileLens.slnx")))
        {
            repository = repository.Parent;
        }

        Assert.IsNotNull(repository, "Tests must run from the local repository build output.");
        // Retain test-owned logs for review under the existing ignored temp directory.
        return Path.Combine(repository.FullName, "temp", "FileLens.BootstrapTests", Guid.NewGuid().ToString("N"));
    }
}
