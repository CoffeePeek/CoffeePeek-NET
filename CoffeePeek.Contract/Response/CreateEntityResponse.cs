namespace CoffeePeek.Contract.Response;

public class CreateEntityResponse<T> : Response<T>
{
    public int? EntityId { get; set; }

    public CreateEntityResponse() { }

    public CreateEntityResponse(bool success, string message, T data, int? entityId = null)
        : base(success, message, data)
    {
        EntityId = entityId;
    }

    /// <summary>
    /// Creates a successful response for entity creation.
    /// </summary>
    public static CreateEntityResponse<T> Success(T data, string message = null, int? entityId = null)
    {
        return new CreateEntityResponse<T>
        {
            IsSuccess = true,
            Message = message ?? "Entity created successfully",
            Data = data,
            EntityId = entityId
        };
    }

    /// <summary>
    /// Creates an error response for entity creation.
    /// </summary>
    public static CreateEntityResponse<T> Error(string message, Dictionary<string, string[]> errors = null, string errorCode = null)
    {
        return new CreateEntityResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            Errors = errors,
            ErrorCode = errorCode
        };
    }
}