using FileLens.Application.DependencyInjection;
using FileLens.UI.ViewModels;
using FileLens.UI.Views;

namespace FileLens.UI.Hosting;

/// <summary>
/// Prepares generic host registration for the UI layer without composing infrastructure.
/// </summary>
public static class UiHostBuilder
{
    /// <summary>
    /// Creates a host builder with UI and application registrations.
    /// </summary>
    /// <param name="args">Optional command-line arguments.</param>
    /// <returns>A configured host builder for later composition-root integration.</returns>
    public static HostApplicationBuilder Create(string[]? args = null)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddApplicationServices();
        builder.Services.AddTransient<MainWindow>();
        builder.Services.AddTransient<MainWindowViewModel>();

        return builder;
    }
}
