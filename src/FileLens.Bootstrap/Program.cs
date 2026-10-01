using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using FileLens.Bootstrap.Hosting;
using FileLens.UI;
using FileLens.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileLens.Bootstrap;

// This entry-point adapter owns composition and framework lifecycle, not feature services.
internal sealed class Program
{
    private IHost? host;
    private App? application;
    private ILogger<Program>? logger;
    private CancellationTokenRegistration stoppingRegistration;
    private bool shutdownStarted;
    private int exitCode;

    [STAThread]
    private static int Main(string[] args) => new Program().Run(args);

    private int Run(string[] args)
    {
        try
        {
            host = BootstrapHostBuilder.Create(args).Build();
            logger = host.Services.GetRequiredService<ILogger<Program>>();
            application = host.Services.GetRequiredService<App>();
            application.InitializeComponent();
            application.Startup += OnStartup;

            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            stoppingRegistration = lifetime.ApplicationStopping.Register(() =>
            {
                // Host signals can originate on another thread; WPF remains dispatcher-owned.
                if (!application.Dispatcher.HasShutdownStarted)
                {
                    application.Dispatcher.BeginInvoke(new Action(OnHostStopping));
                }
            });

            application.Run();
        }
        catch (Exception exception)
        {
            ReportFailure(exception, "Application startup failed.");
            exitCode = 1;
        }
        finally
        {
            stoppingRegistration.Dispose();
            // Normal shutdown disposes asynchronously before leaving the dispatcher.
            // This also covers failure before Run() and unexpected dispatcher termination.
            try
            {
                host?.Dispose();
            }
            catch (Exception exception)
            {
                Trace.TraceError("Host disposal failed ({0}).", exception.GetType().Name);
                exitCode = 1;
            }
        }

        return exitCode;
    }

    private async void OnStartup(object sender, StartupEventArgs args)
    {
        try
        {
            await host!.StartAsync();
            if (shutdownStarted)
            {
                return;
            }

            var window = host.Services.GetRequiredService<MainWindow>();
            application!.MainWindow = window;
            window.Closing += OnWindowClosing;
            window.Show();
            logger!.LogInformation("Main window shown.");
        }
        catch (Exception exception)
        {
            ReportFailure(exception, "Application startup failed.");
            await ShutdownAsync(1);
        }
    }

    private async void OnWindowClosing(object? sender, CancelEventArgs args)
    {
        if (shutdownStarted)
        {
            // Keep the window open until asynchronous Host cleanup has finished.
            args.Cancel = host is not null;
            return;
        }

        args.Cancel = true;
        await ShutdownAsync(0);
    }

    private async void OnHostStopping()
    {
        await ShutdownAsync(0);
    }

    private async Task ShutdownAsync(int requestedExitCode)
    {
        if (shutdownStarted)
        {
            return;
        }

        shutdownStarted = true;
        exitCode = requestedExitCode;
        stoppingRegistration.Dispose();

        try
        {
            logger?.LogInformation("Stopping desktop host.");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await host!.StopAsync(timeout.Token);
        }
        catch (Exception exception)
        {
            ReportFailure(exception, "Application shutdown failed.");
            exitCode = 1;
        }
        finally
        {
            try
            {
                if (host is IAsyncDisposable disposable)
                {
                    await disposable.DisposeAsync();
                }
                else
                {
                    host?.Dispose();
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError("Host disposal failed ({0}).", exception.GetType().Name);
                exitCode = 1;
            }
            finally
            {
                host = null;
                application!.Shutdown(exitCode);
            }
        }
    }

    private void ReportFailure(Exception exception, string operation)
    {
        // Avoid recording raw exception messages, which may contain private paths or settings.
        Trace.TraceError("{0} ({1}).", operation, exception.GetType().Name);
        try
        {
            logger?.LogError("{Operation} ({ExceptionType}).", operation, exception.GetType().Name);
        }
        catch (Exception)
        {
            // Logging must not prevent the startup failure dialog or resource cleanup.
        }

        MessageBox.Show(
            $"{operation}\nFileLens will close. Error type: {exception.GetType().Name}.",
            "FileLens", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
