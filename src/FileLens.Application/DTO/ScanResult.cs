namespace FileLens.Application.DTO;

/// <summary>
/// Represents a completed folder scan within its supported scope, not an atomic snapshot.
/// </summary>
/// <param name="RootFolder">The root folder that was scanned.</param>
/// <param name="TotalFolderCount">The number of retained folders, including the root.</param>
/// <param name="TotalFileCount">The total number of files represented by the scan.</param>
/// <param name="TotalSize">The logical length sum of retained files, not allocated or reclaimable disk space.</param>
/// <param name="CompletionStatus">Whether recoverable failures were observed; exclusions alone do not make a scan partial.</param>
/// <param name="FailureCount">The total observed failure events, not the unknown number of missing files.</param>
/// <param name="ExcludedEntryCount">The total observed excluded entries, not their unvisited descendants.</param>
/// <param name="Diagnostics">The retained diagnostic details; the implementation may limit their number.</param>
/// <remarks>
/// Each failure event or excluded entry contributes one diagnostic before retention limits.
/// Details are truncated when their count is less than FailureCount plus ExcludedEntryCount.
/// Root failures and cancellation do not return a ScanResult.
/// </remarks>
public sealed record ScanResult(
    FolderNode RootFolder,
    int TotalFolderCount,
    int TotalFileCount,
    long TotalSize,
    ScanCompletionStatus CompletionStatus,
    long FailureCount,
    long ExcludedEntryCount,
    IReadOnlyList<ScanDiagnostic> Diagnostics);
