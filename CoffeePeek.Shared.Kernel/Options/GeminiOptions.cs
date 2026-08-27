using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Shared.Kernel.Options;

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gemini-3.6-flash";

    [Url]
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";

    [Range(5, 180)]
    public int TimeoutSeconds { get; set; } = 90;

    /// <summary>
    /// Optional outbound HTTP/HTTPS/SOCKS proxy for Gemini calls.
    /// Google blocks the Generative Language API from some regions (including RU);
    /// set this to a proxy whose egress IP is in a supported country.
    /// Example: <c>socks5://user:pass@proxy.example:1080</c>.
    /// </summary>
    public string ProxyUrl { get; set; } = string.Empty;
}

public class MenuPriceRangeOptions
{
    public string Currency { get; set; } = "BYN";

    public decimal CheapBelow { get; set; } = 7.00m;

    public decimal ExpensiveAbove { get; set; } = 9.00m;
}
