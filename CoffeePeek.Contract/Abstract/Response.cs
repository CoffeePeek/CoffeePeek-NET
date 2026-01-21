namespace CoffeePeek.Contract.Abstract;

public class Response
{
    public bool IsSuccess { get; set; }
    
    public string Message { get; init; }
    
    public object Data { get; set; }
    
    public int StatusCode { get; set; }
    public string ErrorCode {get; init;}
    public List<string>? Errors { get; set; }
    public string StackTrace { get; set; }
    public string InnerException { get; set; }

    public Response() { }

    public Response(bool success, string message, object data)
    {
        IsSuccess = success;
        Message = message;
        Data = data;
    }
    
    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static Response Success(object data = null, string message = null, int statusCode = 200)
    {
        return new Response
        {
            IsSuccess = true,
            Message = message ?? "Operation successful",
            Data = null,
            StatusCode = statusCode
        };
    }
    
    [Obsolete]
    public static Response Error(string message)
    {
        return new Response
        {
            IsSuccess = false,
            Message = message,
            Data = null,
        };
    }
    
    /// <summary>
    /// Creates an error response with message and optional validation errors.
    /// </summary>
    public static Response Error(int statusCode, string message, List<string>? errors = null)
    {
        return new Response
        {
            IsSuccess = false,
            Message = message,
            Data = null,
            StatusCode =  statusCode,
            Errors = errors
        };
    }
    
    /// <summary>
    /// Creates a response from an exception.
    /// </summary>
    public static Response FromException(Exception ex)
    {
        return new Response
        {
            IsSuccess = false,
            Message = ex.Message,
            Data = default,
        };
    }
}