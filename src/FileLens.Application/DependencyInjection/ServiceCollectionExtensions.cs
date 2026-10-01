using FileLens.Application.Interfaces;
using FileLens.Application.Services;

namespace FileLens.Application.DependencyInjection;

/// <summary>
/// Provides application-layer service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers application-layer services.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IScanFolderUseCase, ScanFolderUseCase>();
        return services;
    }
}
