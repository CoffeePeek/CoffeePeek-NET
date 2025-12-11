namespace CoffeePeek.ModerationService.Services.Interfaces;

public interface IYandexGeocodingService
{
    /// <summary>
/// Выполняет геокодирование текстового адреса и получает координаты местоположения.
/// </summary>
/// <param name="address">Текстовый адрес для геокодирования (например, улица, город, дом).</param>
/// <returns>Объект <see cref="GeocodingResult"/> с полями Latitude и Longitude, или <c>null</c>, если по адресу не удалось получить координаты.</returns>
Task<GeocodingResult?> GeocodeAsync(string address, CancellationToken cancellationToken = default);
}

public record GeocodingResult(
    decimal Latitude,
    decimal Longitude
);
