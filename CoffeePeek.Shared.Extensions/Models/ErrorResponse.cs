namespace CoffeePeek.Shared.Extensions.Models;

public class ErrorResponse
{
    public string Message { get; set; }
    public string TraceId { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    
    // Детали только для Development
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
}