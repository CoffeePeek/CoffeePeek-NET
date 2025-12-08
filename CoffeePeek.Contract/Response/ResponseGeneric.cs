namespace CoffeePeek.Contract.Response;

public class Response<TData> : Response
{
    public new TData Data
    {
        get => base.Data is TData data ? data : default(TData);
        set => base.Data = value;
    }

    public Response() { }

    public Response(bool success, string message, TData data)
        : base(success, message, data)
    {
    }

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    public static Response<TData> Success(TData data, string message = null)
    {
        return new Response<TData>
        {
            IsSuccess = true,
            Message = message ?? "Operation successful",
            Data = data
        };
    }

    /// <summary>
    /// Creates an error response with message and optional validation errors.
    /// </summary>
    public static Response<TData> Error(string message, Dictionary<string, string[]> errors = null, string errorCode = null)
    {
        return new Response<TData>
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
    public static Response<TData> FromException(Exception ex, string traceId = null, string errorCode = null, Dictionary<string, string[]> errors = null)
    {
        return new Response<TData>
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