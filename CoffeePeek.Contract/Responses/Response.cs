using CoffeePeek.Contract.Responses;

namespace CoffeePeek.Contract.Response;

public class Response
{
    public bool IsSuccess { get; set; }
    
    public string Message { get; init; }
    
    public object Data { get; set; }
    
    public string ErrorCode { get; set; }
    
    public string TraceId { get; set; }
    
    public Dictionary<string, string[]> Errors { get; set; }

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
    public static Response Success(object data = null, string message = null)
    {
        return new Response
        {
            IsSuccess = true,
            Message = message ?? "Operation successful",
            Data = null
        };
    }
    
    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static Response<T> Success<T>(T data, string message = null)
    {
        return new Response<T>
        {
            IsSuccess = true,
            Message = message ?? "Operation successful",
            Data = data
        };
    }

    /// <summary>
    /// Creates an error response with message and optional validation errors.
    /// </summary>
    public static Response Error(string message, Dictionary<string, string[]> errors = null, string errorCode = null)
    {
        return new Response
        {
            IsSuccess = false,
            Message = message,
            Data = null,
            Errors = errors,
            ErrorCode = errorCode
        };
    }
    
    /// <summary>
    /// Creates an error response with message and optional validation errors.
    /// </summary>
    public static Response<T> Error<T>(string message, Dictionary<string, string[]> errors = null, string errorCode = null)
    {
        return new Response<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            Errors = errors,
            ErrorCode = errorCode
        };
    }

    /// <summary>
    /// Creates a response from an exception.
    /// </summary>
    public static Response<T> FromException<T>(Exception ex, string traceId = null, string errorCode = null, Dictionary<string, string[]> errors = null)
    {
        return new Response<T>
        {
            IsSuccess = false,
            Message = ex.Message,
            TraceId = traceId,
            Data = default,
            ErrorCode = errorCode,
            Errors = errors
        };
    }
}