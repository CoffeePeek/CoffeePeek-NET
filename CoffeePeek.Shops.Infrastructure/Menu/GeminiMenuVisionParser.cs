using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeePeek.Shared.Kernel.Options;
using CoffeePeek.Shops.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Shops.Infrastructure.Menu;

public sealed class GeminiMenuVisionParser(
    IHttpClientFactory httpClientFactory,
    IOptions<GeminiOptions> options,
    ILogger<GeminiMenuVisionParser> logger) : IMenuVisionParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<MenuVisionParseResult> ParseAsync(
        IReadOnlyList<MenuVisionPhoto> photos,
        CancellationToken ct = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            logger.LogError("Menu parse skipped: Gemini API key is not configured (model {Model})", settings.Model);
            return new MenuVisionParseResult(false, "Gemini API key is not configured.", []);
        }

        if (photos.Count == 0)
        {
            logger.LogWarning("Menu parse skipped: no menu photos supplied");
            return new MenuVisionParseResult(false, "No menu photos supplied.", []);
        }

        var parts = new List<object>
        {
            new { text = Prompt }
        };

        foreach (var photo in photos.Take(4))
        {
            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = string.IsNullOrWhiteSpace(photo.ContentType) ? "image/jpeg" : photo.ContentType,
                    data = Convert.ToBase64String(photo.Bytes)
                }
            });
        }

        var payload = new
        {
            contents = new[] { new { parts } },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = Schema
            }
        };

        var url =
            $"{settings.BaseUrl.TrimEnd('/')}/models/{settings.Model}:generateContent?key={Uri.EscapeDataString(settings.ApiKey)}";

        try
        {
            using var client = httpClientFactory.CreateClient("gemini");
            using var response = await client.PostAsJsonAsync(url, payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                logger.LogError(
                    "Gemini menu parse HTTP {StatusCode} for model {Model}: {Body}",
                    (int)response.StatusCode,
                    settings.Model,
                    Trim(body));
                return new MenuVisionParseResult(false, FormatHttpError((int)response.StatusCode, body), []);
            }

            var gemini = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, ct);
            var text = gemini?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                logger.LogError("Gemini menu parse returned an empty response for model {Model}", settings.Model);
                return new MenuVisionParseResult(false, "Gemini returned an empty response.", []);
            }

            ParsedMenuJson? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<ParsedMenuJson>(text, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Gemini menu parse JSON was invalid for model {Model}", settings.Model);
                return new MenuVisionParseResult(false, $"Gemini JSON was invalid: {ex.Message}", []);
            }

            var drinks = (parsed?.Drinks ?? [])
                .Where(d => !string.IsNullOrWhiteSpace(d.RawName))
                .Select(d => new VisionDrinkLine(d.RawName!.Trim(), d.Price, d.VolumeMl, d.Confidence))
                .ToArray();

            logger.LogInformation(
                "Gemini menu parse succeeded for model {Model}: {DrinkCount} drinks from {PhotoCount} photos",
                settings.Model,
                drinks.Length,
                photos.Count);

            return new MenuVisionParseResult(true, null, drinks);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini menu parse threw for model {Model}", settings.Model);
            return new MenuVisionParseResult(false, $"Gemini request failed: {ex.Message}", []);
        }
    }

    private static string FormatHttpError(int statusCode, string body)
    {
        if (body.Contains("User location is not supported", StringComparison.OrdinalIgnoreCase))
        {
            return "Gemini is blocked for this server IP. Set GeminiOptions:ProxyUrl to an HTTP or SOCKS proxy whose egress is in a supported region.";
        }

        return $"Gemini HTTP {statusCode}: {Trim(body)}";
    }

    private static string Trim(string value) =>
        value.Length <= 300 ? value : value[..300];

    private const string Prompt =
        """
        Extract coffee drinks from this cafe menu photo. Ignore food, desserts, merch.
        Return JSON only. Prices are Belarusian rubles (BYN) unless another currency is explicit.
        rawName is the drink name as printed. volumeMl if a size in ml is printed, else null.
        """;

    private static readonly object Schema = new
    {
        type = "object",
        properties = new
        {
            drinks = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        rawName = new { type = "string" },
                        price = new { type = "number" },
                        volumeMl = new { type = "integer" },
                        confidence = new { type = "number" }
                    },
                    required = new[] { "rawName" }
                }
            }
        },
        required = new[] { "drinks" }
    };

    private sealed class ParsedMenuJson
    {
        public List<ParsedDrinkJson> Drinks { get; set; } = [];
    }

    private sealed class ParsedDrinkJson
    {
        public string? RawName { get; set; }
        public decimal? Price { get; set; }
        public int? VolumeMl { get; set; }
        public double? Confidence { get; set; }
    }

    private sealed class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
