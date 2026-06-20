namespace CoffeePeek.Shared.Kernel.Response;

public class ErrorResponse : Response
{
    /// <summary>Machine-readable error code the frontend can switch on (e.g. "SHOP_DUPLICATE").</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Field-level validation errors keyed by field name.</summary>
    public Dictionary<string, string[]>? Errors { get; init; }

#if DEBUG
    public string? StackTrace { get; init; }
    public string? InnerException { get; init; }
#endif

    public ErrorResponse(string message, string? errorCode = null, Dictionary<string, string[]>? errors = null)
        : base(success: false, message, null)
    {
        ErrorCode = errorCode;
        Errors = errors;
    }
}
