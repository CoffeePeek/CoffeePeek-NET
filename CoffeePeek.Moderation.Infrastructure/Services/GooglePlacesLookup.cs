using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Moderation.Infrastructure.Services;

public class GooglePlacesLookup(HttpClient httpClient, IOptions<GooglePlaces> options) : IGooglePlacesLookup
{
    private const string FieldMask =
        "places.displayName,places.formattedAddress,places.businessStatus,places.location,places.googleMapsUri";

    public async Task<GooglePlaceLookupResult> LookupAsync(
        string name,
        decimal latitude,
        decimal longitude,
        CancellationToken ct = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new InvalidOperationException("GooglePlaces:ApiKey is not configured.");

        var payload = new
        {
            textQuery = name,
            languageCode = "ru",
            regionCode = "BY",
            maxResultCount = 5,
            locationBias = new
            {
                circle = new
                {
                    center = new { latitude = (double)latitude, longitude = (double)longitude },
                    radius = 400.0
                }
            }
        };

        var url = $"{settings.BaseUrl.TrimEnd('/')}/places:searchText";
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.TryAddWithoutValidation("X-Goog-Api-Key", settings.ApiKey);
        request.Headers.TryAddWithoutValidation("X-Goog-FieldMask", FieldMask);

        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<PlacesSearchResponse>(ct);
        var now = DateTimeOffset.UtcNow;
        var places = body?.Places ?? [];
        if (places.Count == 0)
            return new GooglePlaceLookupResult(ImportGoogleBusinessStatus.NotFound, null, now);

        GooglePlace? best = null;
        var bestDistance = double.MaxValue;
        foreach (var place in places)
        {
            if (place.Location is null)
                continue;

            var distance = HaversineMeters(
                (double)latitude,
                (double)longitude,
                place.Location.Latitude,
                place.Location.Longitude);

            if (distance < bestDistance)
            {
                best = place;
                bestDistance = distance;
            }
        }

        if (best is null)
            return new GooglePlaceLookupResult(ImportGoogleBusinessStatus.NotFound, null, now);

        if (bestDistance > settings.MaxDistanceMeters)
            return new GooglePlaceLookupResult(ImportGoogleBusinessStatus.Far, best.GoogleMapsUri, now);

        var status = best.BusinessStatus switch
        {
            "OPERATIONAL" => ImportGoogleBusinessStatus.Operational,
            "CLOSED_PERMANENTLY" => ImportGoogleBusinessStatus.ClosedPermanently,
            "CLOSED_TEMPORARILY" => ImportGoogleBusinessStatus.ClosedTemporarily,
            _ => ImportGoogleBusinessStatus.Unknown
        };

        return new GooglePlaceLookupResult(status, best.GoogleMapsUri, now);
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earth = 6371000;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * earth * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    private sealed class PlacesSearchResponse
    {
        [JsonPropertyName("places")]
        public List<GooglePlace> Places { get; set; } = [];
    }

    private sealed class GooglePlace
    {
        [JsonPropertyName("businessStatus")]
        public string? BusinessStatus { get; set; }

        [JsonPropertyName("googleMapsUri")]
        public string? GoogleMapsUri { get; set; }

        [JsonPropertyName("location")]
        public GoogleLatLng? Location { get; set; }
    }

    private sealed class GoogleLatLng
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
}
