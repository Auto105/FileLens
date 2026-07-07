using FileLens.Application.DTO;

namespace FileLens.Application.Interfaces;

/// <summary>
/// Defines the application-layer contract for scanning a folder.
/// </summary>
public interface IFolderScanner
{
    /// <summary>
    /// Scans the specified folder.
    /// </summary>
    /// <param name="folderPath">The absolute or relative path of the folder to scan.</param>
    /// <param name="cancellationToken">A token that can cancel the scan operation.</param>
    /// <returns>A task that represents the asynchronous scan operation and its completed result.</returns>
    Task<ScanResult> ScanAsync(string folderPath, CancellationToken cancellationToken = default);
}
