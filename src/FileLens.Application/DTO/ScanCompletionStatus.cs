namespace FileLens.Application.DTO;

/// <summary>
/// Describes completion within the scanner's supported scope, not snapshot consistency.
/// </summary>
public enum ScanCompletionStatus
{
    /// <summary>No recoverable failures were observed; policy exclusions may exist.</summary>
    Complete,

    /// <summary>Recoverable failures were observed and some scan data may be missing.</summary>
    Partial
}
