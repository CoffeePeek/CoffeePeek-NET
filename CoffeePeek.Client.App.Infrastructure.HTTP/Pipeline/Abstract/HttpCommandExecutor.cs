using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Serialization;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;

public sealed class HttpCommandExecutor(HttpPipeline httpPipeline) : IHttpCommandExecutor
{
    public async Task<ApiResponse> Execute(HttpCommand command, CancellationToken ct)
    {
        try
        {
            var httpResult = await httpPipeline.Send(command, ct);
            return MapFromPipeline(httpResult);
        }
        finally
        {
            command.ResponsePayloadType = null;
            command.SkipUnauthorizedRefreshRetry = false;
        }
    }

    public async Task<ApiResponse<T>> Execute<T>(HttpCommand command, CancellationToken ct)
    {
        try
        {
            command.ResponsePayloadType = typeof(T);
            var httpResult = await httpPipeline.Send(command, ct);
            return MapFromPipeline<T>(httpResult);
        }
        finally
        {
            command.ResponsePayloadType = null;
            command.SkipUnauthorizedRefreshRetry = false;
        }
    }

    private static ApiResponse MapFromPipeline(HttpResult http)
    {
        var response = new ApiResponse
        {
            StatusCode = (int)http.StatusCode,
            IsSuccess = http.IsSuccess,
            Message = http.ErrorMessage
        };

        if (http.DeserializationError is not null)
        {
            response.IsSuccess = false;
            response.Message = http.DeserializationError;
            return response;
        }

        if (http.DeserializedEnvelope is ApiEnvelopeShell shell)
        {
            response.IsSuccess = http.IsSuccess && shell.IsSuccess;
            response.Message = shell.Message ?? response.Message;
            return response;
        }

        if (http.DeserializedEnvelope is not null)
            return response;

        return response;
    }

    private static ApiResponse<T> MapFromPipeline<T>(HttpResult http)
    {
        var response = new ApiResponse<T>
        {
            StatusCode = (int)http.StatusCode,
            IsSuccess = http.IsSuccess,
            Message = http.ErrorMessage
        };

        if (http.DeserializationError is not null)
        {
            response.IsSuccess = false;
            response.Message = http.DeserializationError;
            return response;
        }

        if (http.DeserializedEnvelope is ApiEnvelopeDto<T> envelope)
        {
            response.IsSuccess = http.IsSuccess && envelope.IsSuccess;
            response.Message = envelope.Message ?? response.Message;
            response.Data = envelope.Data;
            return response;
        }

        response.IsSuccess = false;
        if (http.DeserializedEnvelope is not null)
            response.Message ??= "Response shape does not match the expected envelope.";
        else if (!string.IsNullOrWhiteSpace(http.RawBody))
            response.Message ??= "Failed to map deserialized response.";
        else if (http.IsSuccess)
            response.Message ??= "Empty response body.";

        return response;
    }
}
