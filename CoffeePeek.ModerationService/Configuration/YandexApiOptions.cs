using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.ModerationService.Configuration;

public class YandexApiOptions
{
    [Required] public string ApiKey { get; set; } = string.Empty;
    [Required] public string BaseUrl { get; set; } = "https://geocode-maps.yandex.ru/1.x/";
    [Range(1, 300)] public int TimeoutSeconds { get; set; } = 30;
}