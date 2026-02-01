using System.Net.Http.Json;
using CoffeePeek.Contract.Dtos.Yandex;
using CoffeePeek.Moderation.Application.Abstractions;
using CoffeePeek.Moderation.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoffeePeek.Moderation.Infrastructure.Services;

public class YandexGeocodingService(
    HttpClient httpClient,
    IOptions<YandexApiOptions> options,
    ILogger<YandexGeocodingService> logger) : IYandexGeocodingService
{
    private readonly YandexApiOptions _options = options.Value;

    public async Task<GeocodingResult?> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            logger.LogWarning("Attempted to geocode empty address");
            return null;
        }

        try
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{_options.BaseUrl}?apikey={_options.ApiKey}&geocode={encodedAddress}&format=json&results=1";

            var response = await httpClient.GetFromJsonAsync<YandexGeocodingResponse>(url, cancellationToken);

            if (response?.Response?.GeoObjectCollection?.FeatureMember == null ||
                response.Response.GeoObjectCollection.FeatureMember.Count == 0)
            {
                logger.LogWarning("No geocoding results found for address: {Address}", address);
                return null;
            }

            var geoObject = response.Response.GeoObjectCollection.FeatureMember[0].GeoObject;
            if (geoObject?.Point?.Pos == null)
            {
                logger.LogWarning("Invalid geocoding response structure for address: {Address}", address);
                return null;
            }

            var coordinates = geoObject.Point.Pos.Split(' ');
            if (coordinates.Length == 2 &&
                decimal.TryParse(coordinates[1], out var latitude) &&
                decimal.TryParse(coordinates[0], out var longitude))
            {
                return new GeocodingResult(latitude, longitude);
            }
            logger.LogWarning("Invalid coordinates format in geocoding response for address: {Address}", address);
            return null;

        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error while geocoding address: {Address}", address);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Timeout while geocoding address: {Address}", address);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while geocoding address: {Address}", address);
            return null;
        }
    }
}

