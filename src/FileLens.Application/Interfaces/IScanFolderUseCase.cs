using FileLens.Application.DTO;

namespace FileLens.Application.Interfaces;

/// <summary>
/// Provides the Application entry point for a caller-requested folder scan.
/// </summary>
public interface IScanFolderUseCase
{
    /// <summary>
    /// Validates pure input conditions and delegates scanning without changing the result.
    /// </summary>
    /// <param name="folderPath">The folder path, forwarded without trimming or normalization.</param>
    /// <param name="cancellationToken">The caller's cancellation token, forwarded unchanged.</param>
    /// <returns>The scanner's original result, including completion status and diagnostics.</returns>
    /// <remarks>
    /// Cancellation is checked before input validation. Validation failures are observed
    /// through the returned task. Filesystem validation and supported-path policies belong
    /// to the scanner. Scanner exceptions and cancellation propagate without wrapping.
    /// No result is synthesized for failure or cancellation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The folder path is null and cancellation has not been requested.</exception>
    /// <exception cref="ArgumentException">The folder path is empty or whitespace-only and cancellation has not been requested.</exception>
    /// <exception cref="OperationCanceledException">Cancellation is requested before invocation or reported by the scanner.</exception>
    Task<ScanResult> ExecuteAsync(string folderPath, CancellationToken cancellationToken = default);
}
