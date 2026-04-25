using System.Net;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;

public sealed record HttpResult
{
    public required HttpStatusCode StatusCode { get; init; }

    public required bool IsSuccess { get; init; }

    /// <summary>Response body as text (empty if none).</summary>
    public string RawBody { get; init; } = string.Empty;

    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Filled by <see cref="Pipeline.DeserializeResponseBehavior"/>: <see cref="Serialization.ApiEnvelopeShell"/>
    /// or <see cref="Serialization.ApiEnvelopeDto{T}"/> (boxed as <see cref="object"/>).
    /// </summary>
    public object? DeserializedEnvelope { get; init; }

    /// <summary>Set when JSON deserialization fails.</summary>
    public string? DeserializationError { get; init; }
}
