using System.Globalization;
using System.Text.Json;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

namespace CoffeePeek.Moderation.Domain.Import;

public static class CoffeeMapCafeParser
{
    private static readonly (string JsonKey, string OsmDay)[] Days =
    [
        ("mo", "Mo"), ("mon", "Mo"),
        ("tu", "Tu"), ("tue", "Tu"),
        ("we", "We"), ("wed", "We"),
        ("th", "Th"), ("thu", "Th"),
        ("fr", "Fr"), ("fri", "Fr"),
        ("sa", "Sa"), ("sat", "Sa"),
        ("su", "Su"), ("sun", "Su")
    ];

    public static IReadOnlyList<CoffeeMapCandidateSnapshot> Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return Parse(document.RootElement);
    }

    public static IReadOnlyList<CoffeeMapCandidateSnapshot> Parse(JsonElement root)
    {
        var cafes = root.ValueKind switch
        {
            JsonValueKind.Array => root.EnumerateArray(),
            JsonValueKind.Object when root.TryGetProperty("cafes", out var nested) && nested.ValueKind == JsonValueKind.Array
                => nested.EnumerateArray(),
            _ => throw new JsonException("CoffeeMap payload must be a cafe array or an object with a cafes array.")
        };

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<CoffeeMapCandidateSnapshot>();

        foreach (var cafe in cafes)
        {
            if (cafe.ValueKind != JsonValueKind.Object)
                continue;

            var id = ReadString(cafe, "id");
            if (string.IsNullOrWhiteSpace(id) || !seen.Add(id))
                continue;

            var lat = ReadDouble(cafe, "lat");
            var lng = ReadDouble(cafe, "lng");
            if (lat is null || lng is null)
                continue;

            var website = ReadString(cafe, "website");
            var instagram = ReadString(cafe, "instagram")
                            ?? (website is not null && website.Contains("instagram.com", StringComparison.OrdinalIgnoreCase)
                                ? website
                                : null);

            result.Add(new CoffeeMapCandidateSnapshot(
                ExternalId: id.Trim(),
                Name: ReadString(cafe, "name"),
                Address: ReadString(cafe, "address"),
                Latitude: (decimal)lat.Value,
                Longitude: (decimal)lng.Value,
                Phone: ReadString(cafe, "phone"),
                Website: website,
                Instagram: instagram,
                OpeningHours: FormatHours(cafe),
                GooglePlaceId: ReadString(cafe, "google_place_id"),
                IsSpecialty: ReadBool(cafe, "is_specialty"),
                Recommended: ReadBool(cafe, "recommended"),
                GoogleRating: ReadDouble(cafe, "google_rating"),
                GoogleRatingsCount: ReadInt(cafe, "google_ratings_count"),
                UpdatedAt: ReadTimestamp(cafe, "updated_at"),
                AmenitySignals: AmenitySignals(cafe)));
        }

        return result;
    }

    private static IReadOnlyList<string> AmenitySignals(JsonElement cafe)
    {
        var signals = new List<string>();
        if (ReadBool(cafe, "has_wifi")) signals.Add("coffeemap:wifi");
        if (ReadBool(cafe, "pet_friendly")) signals.Add("coffeemap:pet");
        if (ReadBool(cafe, "has_power_outlets")) signals.Add("coffeemap:power");
        if (ReadBool(cafe, "wheelchair_accessible")) signals.Add("coffeemap:wheelchair");
        return signals;
    }

    private static string? FormatHours(JsonElement cafe)
    {
        if (!cafe.TryGetProperty("hours", out var hours) || hours.ValueKind != JsonValueKind.Object)
            return null;

        var parts = new List<string>();
        foreach (var group in Days.GroupBy(d => d.OsmDay, StringComparer.Ordinal))
        {
            JsonElement day = default;
            var found = false;
            foreach (var (jsonKey, _) in group)
            {
                if (TryGetIgnoreCase(hours, jsonKey, out day) && day.ValueKind == JsonValueKind.Object)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                continue;

            var open = ReadString(day, "open");
            var close = ReadString(day, "close");
            if (open is null || close is null)
                continue;

            parts.Add($"{group.Key} {open}-{close}");
        }

        return parts.Count == 0 ? null : string.Join("; ", parts);
    }

    private static bool TryGetIgnoreCase(JsonElement obj, string name, out JsonElement value)
    {
        foreach (var prop in obj.EnumerateObject())
        {
            if (prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string? ReadString(JsonElement obj, string name)
    {
        if (!TryGetIgnoreCase(obj, name, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;
        var text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    private static bool ReadBool(JsonElement obj, string name) =>
        TryGetIgnoreCase(obj, name, out var value) && value.ValueKind is JsonValueKind.True;

    private static double? ReadDouble(JsonElement obj, string name)
    {
        if (!TryGetIgnoreCase(obj, name, out var value))
            return null;
        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDouble(out var number) => number,
            JsonValueKind.String when double.TryParse(value.GetString(), CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }

    private static int? ReadInt(JsonElement obj, string name)
    {
        if (!TryGetIgnoreCase(obj, name, out var value))
            return null;
        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var number) => number,
            JsonValueKind.String when int.TryParse(value.GetString(), CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null
        };
    }

    private static DateTimeOffset? ReadTimestamp(JsonElement obj, string name)
    {
        var raw = ReadString(obj, name);
        if (raw is null)
            return null;
        return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : null;
    }
}
