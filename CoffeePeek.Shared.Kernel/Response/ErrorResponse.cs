namespace CoffeePeek.Shared.Kernel.Response;

public class ErrorResponse(string message) : Response(success: false, message, null)
{
#if DEBUG
    public string StackTrace { get; set; }
    public string InnerException { get; set; }
#endif
}