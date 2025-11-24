namespace GestionEmps.API.Models;

/// <summary>
/// Represents a standardized error payload returned by the API.
/// </summary>
public class ErrorResponse
{
    /// <summary>Human-readable error message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Machine-readable error code.</summary>
    public string ErrorCode { get; init; } = string.Empty;

    /// <summary>HTTP status code associated with the error.</summary>
    public int StatusCode { get; init; }

    /// <summary>UTC timestamp for the error event.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Correlation ID for tracing the request.</summary>
    public string? TraceId { get; init; }

    /// <summary>Dictionary of validation errors (field → list of messages).</summary>
    public Dictionary<string, List<string>>? ValidationErrors { get; init; }

    // -------------------------------
    // Factory methods
    // -------------------------------

    /// <summary>Creates a generic error response.</summary>
    public static ErrorResponse Create(
        string message,
        string errorCode,
        int statusCode,
        string? traceId = null)
        => new()
        {
            Message = message,
            ErrorCode = errorCode,
            StatusCode = statusCode,
            TraceId = traceId
        };

    /// <summary>Creates a validation error response.</summary>
    public static ErrorResponse CreateValidation(
        Dictionary<string, List<string>> validationErrors,
        string? traceId = null)
        => new()
        {
            Message = "Erreurs de validation",
            ErrorCode = "VALIDATION_ERROR",
            StatusCode = 400,
            ValidationErrors = validationErrors,
            TraceId = traceId
        };
}
