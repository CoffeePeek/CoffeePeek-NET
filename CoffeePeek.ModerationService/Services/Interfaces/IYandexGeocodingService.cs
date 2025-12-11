namespace CoffeePeek.ModerationService.Services.Interfaces;

public interface IYandexGeocodingService
{
    Task<GeocodingResult?> GeocodeAsync(string address, CancellationToken cancellationToken = default);
}

public record GeocodingResult(
    decimal Latitude,
    decimal Longitude
);

