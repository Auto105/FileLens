using System.IO;
using FileLens.Application.DependencyInjection;
using FileLens.Infrastructure.DependencyInjection;
using FileLens.UI.Hosting;
using Microsoft.Extensions.Hosting;

namespace FileLens.Bootstrap.Hosting;

/// <summary>
/// Composes the production desktop host without creating WPF objects.
/// </summary>
public static class BootstrapHostBuilder
{
    /// <summary>
    /// Creates a host builder with Application, Infrastructure, and UI registrations.
    /// </summary>
    /// <param name="args">Optional command-line configuration arguments.</param>
    /// <returns>The configured builder, ready for validation or building.</returns>
    /// <remarks>
    /// FileLens:Logging:LogDirectory can override the existing logging default.
    /// Relative logging directories are rooted under the user's local FileLens data directory.
    /// Absolute directories are preserved. WPF initialization belongs to the entry point.
    /// </remarks>
    public static HostApplicationBuilder Create(string[]? args = null)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(configureLogging: options =>
        {
            var directory = builder.Configuration["FileLens:Logging:LogDirectory"] ?? options.LogDirectory;
            if (!Path.IsPathFullyQualified(directory))
            {
                var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrEmpty(applicationData))
                {
                    throw new InvalidOperationException("The local application-data directory is unavailable.");
                }

                directory = Path.GetFullPath(directory, Path.Combine(applicationData, "FileLens"));
            }

            options.LogDirectory = directory;
        });
        builder.Services.AddUiServices();

        return builder;
    }
}
