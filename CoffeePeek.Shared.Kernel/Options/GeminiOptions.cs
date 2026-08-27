using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Shared.Kernel.Options;

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gemini-3.6-flash";

    [Url]
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";

    [Range(5, 180)]
    public int TimeoutSeconds { get; set; } = 60;
}

public class MenuPriceRangeOptions
{
    public string Currency { get; set; } = "BYN";

    public decimal CheapBelow { get; set; } = 7.00m;

    public decimal ExpensiveAbove { get; set; } = 9.00m;
}
