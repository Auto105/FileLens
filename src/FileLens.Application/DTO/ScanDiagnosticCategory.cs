namespace FileLens.Application.DTO;

/// <summary>
/// Provides provider-independent reasons for scan failures and policy exclusions.
/// </summary>
public enum ScanDiagnosticCategory
{
    /// <summary>Access to an entry was denied.</summary>
    AccessDenied,

    /// <summary>An entry was no longer present when accessed.</summary>
    NotFound,

    /// <summary>An input/output operation failed.</summary>
    IoError,

    /// <summary>An entry was excluded by the reparse point policy.</summary>
    ReparsePointExcluded,

    /// <summary>An entry was excluded by the protected directory policy.</summary>
    ProtectedDirectoryExcluded,

    /// <summary>An entry requires path or storage behavior outside the supported scope.</summary>
    UnsupportedPath
}
