namespace CoffeePeek.Shops.Domain;

public record Result
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

public record Result<T> : Result
{
    public T? Data { get; init; }
    
    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public new static Result<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}