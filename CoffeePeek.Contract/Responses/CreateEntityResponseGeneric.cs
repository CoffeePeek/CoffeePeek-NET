namespace CoffeePeek.Contract.Responses;

public class CreateEntityResponse : Response
{
    /// <summary>
    /// Creates a successful response for entity creation.
    /// </summary>
    public static CreateEntityResponse Success(string message = null)
    {
        return new CreateEntityResponse
        {
            IsSuccess = true,
            Message = message ?? "Entity created successfully",
        };
    }

    /// <summary>
    /// Creates an error response for entity creation.
    /// </summary>
    public new static CreateEntityResponse Error(string message)
    {
        return new CreateEntityResponse
        {
            IsSuccess = false,
            Message = message,
            Data = null,
        };
    }
}