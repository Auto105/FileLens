namespace FileLens.Application.DTO;

/// <summary>
/// Distinguishes an observed scan failure from an intentional policy exclusion.
/// </summary>
public enum ScanDiagnosticKind
{
    /// <summary>An operation failed while processing a supported entry.</summary>
    Failure,

    /// <summary>An observed entry was excluded by scanner policy.</summary>
    Excluded
}
