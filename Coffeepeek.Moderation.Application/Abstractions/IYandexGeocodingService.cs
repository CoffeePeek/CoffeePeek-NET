using Coffeepeek.Moderation.Application.Common.Models;

namespace Coffeepeek.Moderation.Application.Abstractions;

public interface IYandexGeocodingService
{
    Task<GeocodingResult?> GeocodeAsync(string address, CancellationToken cancellationToken = default);
}