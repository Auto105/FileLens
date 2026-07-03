using FileLens.Infrastructure.Configuration;

namespace FileLens.Infrastructure.DependencyInjection;

/// <summary>
/// Provides infrastructure-layer registration helpers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers infrastructure-layer configuration scaffolding.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="configureDatabase">Optional database configuration.</param>
    /// <param name="configureLogging">Optional logging configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        Action<FileLensDatabaseOptions>? configureDatabase = null,
        Action<FileLensLoggingOptions>? configureLogging = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddFileLensSqlite(configureDatabase);
        services.AddFileLensLogging(configureLogging);

        return services;
    }

    /// <summary>
    /// Registers SQLite configuration scaffolding for later repository composition.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="configureDatabase">Optional database configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddFileLensSqlite(
        this IServiceCollection services,
        Action<FileLensDatabaseOptions>? configureDatabase = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<FileLensDatabaseOptions>();

        if (configureDatabase is not null)
        {
            services.Configure(configureDatabase);
        }

        return services;
    }

    /// <summary>
    /// Registers file-based logging scaffolding for later host composition.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="configureLogging">Optional logging configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddFileLensLogging(
        this IServiceCollection services,
        Action<FileLensLoggingOptions>? configureLogging = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<FileLensLoggingOptions>();

        if (configureLogging is not null)
        {
            services.Configure(configureLogging);
        }

        services.AddLogging();
        services.AddSerilog(
            (serviceProvider, loggerConfiguration) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FileLensLoggingOptions>>().Value;
                var logDirectory = Path.GetFullPath(options.LogDirectory);

                Directory.CreateDirectory(logDirectory);

                loggerConfiguration
                    .MinimumLevel.Is(options.MinimumLevel)
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        Path.Combine(logDirectory, options.LogFileName),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: options.RetainedFileCountLimit);
            },
            preserveStaticLogger: false,
            writeToProviders: false);

        return services;
    }
}
