namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public class ApiResponse
{
    public bool IsSuccess { get; set; }

    public string? Message { get; set; }

    public int StatusCode { get; set; }
}

public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}
