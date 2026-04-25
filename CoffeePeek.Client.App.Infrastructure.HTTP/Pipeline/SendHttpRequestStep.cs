using System.Text;
using System.Text.Json;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline;

/// <summary>Innermost step: performs <see cref="HttpClient.SendAsync"/> and does not call <paramref name="next"/>.</summary>
public sealed class SendHttpRequestStep(HttpClient http) : IHttpPipelineStep
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<HttpResult> Execute(
        HttpCommand command,
        Func<HttpCommand, Task<HttpResult>> next,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(command.Method, BuildUri(command));

        foreach (var (key, value) in command.Headers)
            request.Headers.TryAddWithoutValidation(key, value);

        if (command.Body is not null && command.Method != HttpMethod.Get && command.Method != HttpMethod.Head)
        {
            var json = JsonSerializer.Serialize(command.Body, command.Body.GetType(), SerializerOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        return new HttpResult
        {
            StatusCode = response.StatusCode,
            IsSuccess = response.IsSuccessStatusCode,
            RawBody = body,
            ErrorMessage = response.IsSuccessStatusCode ? null : TryReadErrorMessage(body, response.ReasonPhrase)
        };
    }

    private static Uri BuildUri(HttpCommand command)
    {
        var path = command.Endpoint.TrimStart('/');
        if (command.Query.Count == 0)
            return new Uri(path, UriKind.Relative);

        var qs = string.Join('&',
            command.Query.Select(p =>
                $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        return new Uri($"{path}?{qs}", UriKind.Relative);
    }

    private static string? TryReadErrorMessage(string json, string? fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
            return fallback;

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

        return fallback;
    }
}
