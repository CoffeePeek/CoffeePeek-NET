using System.Text.Json;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Serialization;

/// <summary>Server envelope without strongly typed <c>data</c> (payload as <see cref="JsonElement"/>).</summary>
public sealed class ApiEnvelopeShell
{
    public bool IsSuccess { get; set; }

    public string? Message { get; set; }

    public JsonElement Data { get; set; }
}
