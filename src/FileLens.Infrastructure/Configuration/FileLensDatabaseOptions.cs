namespace FileLens.Infrastructure.Configuration;

/// <summary>
/// Defines SQLite persistence settings for the application.
/// </summary>
public sealed class FileLensDatabaseOptions
{
    /// <summary>
    /// Gets or sets the relative or absolute path to the SQLite database file.
    /// </summary>
    public string DatabasePath { get; set; } = Path.Combine("data", "filelens.db");
}
