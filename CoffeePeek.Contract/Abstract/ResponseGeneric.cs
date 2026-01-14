using System.Net;

namespace CoffeePeek.Contract.Abstract;

public class Response<TData> : Response
{
    public new TData Data
    {
        get => base.Data is TData data ? data : default;
        init => base.Data = value;
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
    [Obsolete]
    public new static Response<TData> Error(string message)
    {
        return new Response<TData>
        {
            IsSuccess = false,
            Message = message,
            Data = default
        };
    }
    
    public static Response<TData> Error(HttpStatusCode statusCode, string message)
    {
        return new Response<TData>
        {
            IsSuccess = false,
            Message = message,
            Data = default
        };
    }

    /// <summary>
    /// Creates a response from an exception.
    /// </summary>
    public new static Response<TData> FromException(Exception ex)
    {
        return new Response<TData>
        {
            IsSuccess = false,
            Message = ex.Message,
            Data = default,
        };
    }
}