using CoffeePeek.Moderation.Application.Common.Models;

namespace CoffeePeek.Moderation.Application.Abstractions;

public interface IYandexGeocodingService
{
    Task<GeocodingResult?> GeocodeAsync(string address, CancellationToken cancellationToken = default);
}