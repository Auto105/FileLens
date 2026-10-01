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
    /// <param name="folderPath">An absolute or ordinary relative directory path; relative paths use the invocation-time base directory.</param>
    /// <param name="cancellationToken">A token that can cancel the scan operation.</param>
    /// <returns>A task that represents the asynchronous scan operation and its completed result.</returns>
    /// <remarks>
    /// Results use absolute paths. Recoverable descendant failures produce a partial result;
    /// policy exclusions alone permit complete status. The scan is not an atomic snapshot.
    /// Windows Sprint 2 MVP excludes network and device paths, ambiguous rooted paths,
    /// protected directories, and reparse points (including some cloud-backed entries).
    /// Implementations document their supported scope and diagnostic retention policy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The folder path is null.</exception>
    /// <exception cref="ArgumentException">The path is blank, invalid, or identifies a file.</exception>
    /// <exception cref="DirectoryNotFoundException">The root is missing or its disappearance is detected during the scan.</exception>
    /// <exception cref="UnauthorizedAccessException">The root cannot be accessed.</exception>
    /// <exception cref="NotSupportedException">The root is outside the supported scanner scope.</exception>
    /// <exception cref="IOException">A root input/output operation fails.</exception>
    /// <exception cref="OperationCanceledException">Cancellation is observed; no partial result is returned.</exception>
    Task<ScanResult> ScanAsync(string folderPath, CancellationToken cancellationToken = default);
}
