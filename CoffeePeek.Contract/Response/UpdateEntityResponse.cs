namespace CoffeePeek.Contract.Response;

public class UpdateEntityResponse<T> : Response<T>
{
    public T OldEntity { get; set; }

    public UpdateEntityResponse() { }

    public UpdateEntityResponse(bool success, string message, T data, T oldEntity = default(T))
        : base(success, message, data)
    {
        OldEntity = oldEntity;
    }

    /// <summary>
    /// Creates a successful response for entity update.
    /// </summary>
    public static UpdateEntityResponse<T> Success(T data, string message = null, T oldEntity = default(T))
    {
        return new UpdateEntityResponse<T>
        {
            IsSuccess = true,
            Message = message ?? "Entity updated successfully",
            Data = data,
            OldEntity = oldEntity
        };
    }

    /// <summary>
    /// Creates an error response for entity update.
    /// </summary>
    public static UpdateEntityResponse<T> Error(string message, Dictionary<string, string[]> errors = null, string errorCode = null)
    {
        return new UpdateEntityResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            Errors = errors,
            ErrorCode = errorCode
        };
    }
}