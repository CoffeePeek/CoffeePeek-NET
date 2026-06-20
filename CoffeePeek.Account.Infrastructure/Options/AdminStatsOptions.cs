namespace CoffeePeek.Account.Infrastructure.Options;

/// <summary>Base URLs for cross-service admin stats aggregation.</summary>
public class AdminStatsOptions
{
    /// <summary>
    /// Shops service base URL (e.g. http://coffeepeekshopsservice.railway.internal).
    /// When empty, falls back to Aspire service discovery name for local dev.
    /// </summary>
    public string? ShopsServiceUrl { get; init; }

    /// <summary>
    /// Moderation service base URL (e.g. http://coffeepeekmoderationservice.railway.internal).
    /// When empty, falls back to Aspire service discovery name for local dev.
    /// </summary>
    public string? ModerationServiceUrl { get; init; }
}
