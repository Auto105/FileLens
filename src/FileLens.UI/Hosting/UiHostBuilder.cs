using FileLens.UI.ViewModels;
using FileLens.UI.Views;

namespace FileLens.UI.Hosting;

/// <summary>
/// Registers UI types without creating a host or composing other layers.
/// </summary>
public static class UiHostBuilder
{
    /// <summary>
    /// Registers the process application and independently created shell components.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddUiServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<App>();
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();

        return services;
    }
}
