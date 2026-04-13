using System.Text.Json;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Serialization;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline;

/// <summary>
/// Runs after transport: parses JSON into <see cref="ApiEnvelopeShell"/> or <see cref="ApiEnvelopeDto{T}"/> and attaches to <see cref="HttpResult"/>.
/// </summary>
public sealed class DeserializeResponseBehavior : IHttpPipelineStep
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<HttpResult> Execute(
        HttpCommand command,
        Func<HttpCommand, Task<HttpResult>> next,
        CancellationToken ct)
    {
        var result = await next(command);

        if (string.IsNullOrWhiteSpace(result.RawBody))
            return result;

        try
        {
            if (command.ResponsePayloadType is null)
            {
                var shell = JsonSerializer.Deserialize<ApiEnvelopeShell>(result.RawBody, JsonOptions);
                if (shell is null)
                    return result with { DeserializationError = "Response deserialized to null (envelope shell)." };
                return result with { DeserializedEnvelope = shell };
            }

            var envelopeType = typeof(ApiEnvelopeDto<>).MakeGenericType(command.ResponsePayloadType);
            var envelope = JsonSerializer.Deserialize(result.RawBody, envelopeType, JsonOptions);
            if (envelope is null)
                return result with { DeserializationError = "Response deserialized to null (typed envelope)." };
            return result with { DeserializedEnvelope = envelope };
        }
        catch (Exception ex)
        {
            return result with
            {
                DeserializationError = ex.Message,
                DeserializedEnvelope = null
            };
        }
    }
}
