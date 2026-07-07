namespace FileLens.Application.DTO;

/// <summary>
/// Represents a scanned file in the application-layer scan contract.
/// </summary>
/// <param name="FilePath">The full path of the scanned file.</param>
/// <param name="FileExtension">The file extension of the scanned file.</param>
/// <param name="FileSize">The size of the scanned file in bytes.</param>
/// <param name="CreationTimeUtc">The UTC creation time of the scanned file.</param>
/// <param name="LastWriteTimeUtc">The UTC last write time of the scanned file.</param>
/// <param name="Attributes">The file system attributes of the scanned file.</param>
public sealed record FileNode(
    string FilePath,
    string FileExtension,
    long FileSize,
    DateTime CreationTimeUtc,
    DateTime LastWriteTimeUtc,
    FileAttributes Attributes);
