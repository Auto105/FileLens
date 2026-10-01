using FileLens.Application.DTO;
using FileLens.Application.Interfaces;

namespace FileLens.Application.Services;

/// <summary>
/// Validates scan requests and delegates filesystem behavior to the injected scanner.
/// </summary>
public sealed class ScanFolderUseCase : IScanFolderUseCase
{
    private readonly IFolderScanner scanner;

    /// <summary>
    /// Creates a use case with its scanner dependency.
    /// </summary>
    /// <param name="scanner">The scanner responsible for filesystem validation and scanning.</param>
    /// <exception cref="ArgumentNullException">The scanner is null.</exception>
    public ScanFolderUseCase(IFolderScanner scanner)
    {
        ArgumentNullException.ThrowIfNull(scanner);
        this.scanner = scanner;
    }

    /// <inheritdoc />
    public async Task<ScanResult> ExecuteAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(folderPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        return await scanner.ScanAsync(folderPath, cancellationToken).ConfigureAwait(false);
    }
}
