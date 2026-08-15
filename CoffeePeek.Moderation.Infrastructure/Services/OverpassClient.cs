using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Moderation.Domain.Import;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Moderation.Infrastructure.Services;

public class OverpassClient(HttpClient httpClient, ILogger<OverpassClient> logger) : IOverpassClient
{
    private static readonly string[] Endpoints =
    [
        "https://overpass-api.de/api/interpreter",
        "https://overpass.kumi.systems/api/interpreter"
    ];

    private const double South = 53.824;
    private const double West = 27.389;
    private const double North = 53.974;
    private const double East = 27.761;

    private static readonly string Query = $"""
        [out:json][timeout:90];
        (
          nwr[amenity=cafe]({South},{West},{North},{East});
          nwr[shop=coffee]({South},{West},{North},{East});
          nwr[amenity=vending_machine][vending=coffee]({South},{West},{North},{East});
        );
        out center meta;
        """;

    public async Task<IReadOnlyList<OsmCandidateSnapshot>> FetchMinskCafesAsync(CancellationToken ct = default)
    {
        Exception? lastError = null;
        foreach (var endpoint in Endpoints)
        {
            try
            {
                using var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["data"] = Query });
                using var response = await httpClient.PostAsync(endpoint, content, ct);
                response.EnsureSuccessStatusCode();

                var payload = await response.Content.ReadFromJsonAsync<OverpassResponse>(ct)
                              ?? throw new InvalidOperationException("Empty Overpass response.");

                return Normalize(payload);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                lastError = ex;
                logger.LogWarning(ex, "Overpass endpoint {Endpoint} failed", endpoint);
            }
        }

        throw new InvalidOperationException($"All Overpass endpoints failed: {lastError?.Message}", lastError);
    }

    private static List<OsmCandidateSnapshot> Normalize(OverpassResponse payload)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<OsmCandidateSnapshot>();

        foreach (var el in payload.Elements)
        {
            if (el.Type is null || el.Id is null)
                continue;

            var externalId = $"{el.Type}/{el.Id}";
            if (!seen.Add(externalId))
                continue;

            var tags = el.Tags ?? new Dictionary<string, string>();
            var (lat, lon) = Coords(el);
            if (lat is null || lon is null)
                continue;

            var name = Get(tags, "name") ?? Get(tags, "name:ru") ?? Get(tags, "name:en");
            var website = Get(tags, "website") ?? Get(tags, "contact:website");

            result.Add(new OsmCandidateSnapshot(
                externalId,
                name,
                Address(tags),
                (decimal)lat.Value,
                (decimal)lon.Value,
                Get(tags, "contact:phone") ?? Get(tags, "phone"),
                website,
                OsmCafeClassifier.InstagramUrl(tags, website),
                Get(tags, "opening_hours"),
                Get(tags, "cuisine"),
                Get(tags, "brand"),
                ParseTimestamp(el.Timestamp),
                Get(tags, "check_date"),
                tags));
        }

        return result;
    }

    private static (double? Lat, double? Lon) Coords(OverpassElement el)
    {
        if (el.Lat is not null && el.Lon is not null)
            return (el.Lat, el.Lon);

        return (el.Center?.Lat, el.Center?.Lon);
    }

    private static string? Address(IReadOnlyDictionary<string, string> tags)
    {
        var parts = new[]
        {
            Get(tags, "addr:street"),
            Get(tags, "addr:housenumber"),
            Get(tags, "addr:city")
        }.Where(p => !string.IsNullOrWhiteSpace(p));

        var line = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(line) ? Get(tags, "addr:full") : line;
    }

    private static DateTimeOffset? ParseTimestamp(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : null;
    }

    private static string? Get(IReadOnlyDictionary<string, string> tags, string key) =>
        tags.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private sealed class OverpassResponse
    {
        [JsonPropertyName("elements")]
        public List<OverpassElement> Elements { get; set; } = [];
    }

    private sealed class OverpassElement
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("lat")]
        public double? Lat { get; set; }

        [JsonPropertyName("lon")]
        public double? Lon { get; set; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("tags")]
        public Dictionary<string, string>? Tags { get; set; }

        [JsonPropertyName("center")]
        public OverpassCenter? Center { get; set; }
    }

    private sealed class OverpassCenter
    {
        [JsonPropertyName("lat")]
        public double? Lat { get; set; }

        [JsonPropertyName("lon")]
        public double? Lon { get; set; }
    }
}
