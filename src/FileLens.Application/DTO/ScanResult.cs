namespace FileLens.Application.DTO;

/// <summary>
/// Represents the completed application-layer result of a folder scan.
/// </summary>
/// <param name="RootFolder">The root folder that was scanned.</param>
/// <param name="TotalFolderCount">The total number of folders represented by the scan.</param>
/// <param name="TotalFileCount">The total number of files represented by the scan.</param>
/// <param name="TotalSize">The total size of all scanned files in bytes.</param>
public sealed record ScanResult(
    FolderNode RootFolder,
    int TotalFolderCount,
    int TotalFileCount,
    long TotalSize);
