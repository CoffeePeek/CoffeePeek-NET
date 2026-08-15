namespace CoffeePeek.Moderation.Infrastructure;

public class GooglePlaces
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://places.googleapis.com/v1/";
    public int TimeoutSeconds { get; set; } = 15;
    public int CacheDays { get; set; } = 30;
    public int MaxDistanceMeters { get; set; } = 250;
}
