using System.Net;
using System.Text.Json;
using CoffeePeek.Client.App.Infrastructure.HTTP.Configuration;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Serialization;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses.Auth;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Services;

public sealed class TokenRefresher(HttpClient http, AuthClientOptions options) : ITokenRefresher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<Result<RefreshTokenResponse>> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var path = options.TokensPath.Trim().TrimStart('/');
        using var request = new HttpRequestMessage(HttpMethod.Put, path);
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var msg = TryReadMessage(body) ?? response.ReasonPhrase ?? "Refresh failed.";
            return Result.Fail(msg);
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<ApiEnvelopeDto<RefreshTokenResponse>>(body, JsonOptions);
            if (envelope is { IsSuccess: true, Data: not null })
                return Result.Ok(envelope.Data);

            var message = envelope?.Message ?? TryReadMessage(body) ?? "Refresh failed.";
            return Result.Fail(message);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Invalid refresh response: {ex.Message}");
        }
    }

    private static string? TryReadMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("message", out var m))
                return m.GetString();
            if (root.TryGetProperty("Message", out var m2))
                return m2.GetString();
        }
        catch
        {
            // ignored
        }

        return null;
    }
}
