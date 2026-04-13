namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Serialization;

public sealed class ApiEnvelopeDto<T>
{
    public bool IsSuccess { get; set; }

    public string? Message { get; set; }

    public T? Data { get; set; }
}
