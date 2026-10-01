namespace FileLens.Application.DTO;

/// <summary>
/// Represents a locally retained diagnostic without an exception object or raw message.
/// Paths may contain sensitive information and must not be uploaded or logged implicitly.
/// </summary>
/// <param name="Path">The absolute path associated with the observed event.</param>
/// <param name="Kind">Whether this event is a failure or policy exclusion.</param>
/// <param name="Category">The reason used to produce a user-facing explanation.</param>
/// <param name="ExceptionType">The optional exception type name for internal troubleshooting.</param>
/// <param name="ErrorCode">The optional exception HRESULT for internal troubleshooting.</param>
public sealed record ScanDiagnostic(
    string Path,
    ScanDiagnosticKind Kind,
    ScanDiagnosticCategory Category,
    string? ExceptionType = null,
    int? ErrorCode = null);
