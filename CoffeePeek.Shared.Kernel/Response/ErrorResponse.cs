namespace CoffeePeek.Shared.Kernel.Response;

public class ErrorResponse(string message) : Response(success: false, message, null)
{
#if DEBUG
    public string? StackTrace { get; init; }
    public string? InnerException { get; init; }
#endif
}
