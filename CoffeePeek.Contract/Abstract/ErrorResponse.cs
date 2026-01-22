namespace CoffeePeek.Contract.Abstract;

public class ErrorResponse(string errorCode, string message) : Response(success: false, message, null)
{
    public string ErrorCode { get; init; } = errorCode;
#if DEBUG
    public string StackTrace { get; set; }
    public string InnerException { get; set; }
#endif
}