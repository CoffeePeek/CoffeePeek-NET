namespace CoffeePeek.Moderation.Contract.Abstract;

public class Response
{
    public bool Success { get; set; }
    
    public string Message { get; set; }
    
    public object Data { get; set; }

    public Response() { }

    public Response(bool success, string message, object data)
    {
        Success = success;
        Message = message;
        Data = data;
    }
    
    public static T SuccessResponse<T>(object data = null, string message = "Operation successful")
        where T : Response, new()
    {
        return new T
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static T ErrorResponse<T>(string message, object data = default)
        where T : Response, new()
    {
        return new T
        {
            Success = false,
            Message = message,
            Data = data
        };
    }
}

public class Response<TData> : Response
{
    public new TData Data
    {
        get => (TData)base.Data;
        set => base.Data = value;
    }

    public Response() { }

    public Response(bool success, string message, TData data)
        : base(success, message, data)
    {
    }

    public static TResponse SuccessResponse<TResponse>(TData data = default, string message = "Operation successful")
        where TResponse : Response<TData>, new()
    {
        return new TResponse
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static TResponse ErrorResponse<TResponse>(string message, TData data = default)
        where TResponse : Response<TData>, new()
    {
        return new TResponse
        {
            Success = false,
            Message = message,
            Data = data
        };
    }
}