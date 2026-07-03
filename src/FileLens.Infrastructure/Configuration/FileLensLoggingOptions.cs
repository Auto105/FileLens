namespace FileLens.Infrastructure.Configuration;

/// <summary>
/// Defines file-based logging settings for the application.
/// </summary>
public sealed class FileLensLoggingOptions
{
    /// <summary>
    /// Gets or sets the directory where log files are written.
    /// </summary>
    public string LogDirectory { get; set; } = Path.Combine("logs");

    /// <summary>
    /// Gets or sets the log file name pattern.
    /// </summary>
    public string LogFileName { get; set; } = "filelens-.log";

    /// <summary>
    /// Gets or sets the minimum Serilog level used for file logging.
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Gets or sets the maximum number of retained rolling log files.
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 14;
}
