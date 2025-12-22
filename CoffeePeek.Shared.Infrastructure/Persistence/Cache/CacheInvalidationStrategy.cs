using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shared.Infrastructure.Persistence;

public sealed class CacheInvalidationStrategy(
    IRedisService redisService,
    ILogger<CacheInvalidationStrategy> logger) : ICacheInvalidationStrategy
{
    public static class Tags
    {
        public const string ShopsDictionary = "shops:dictionary";
        public const string ShopsLists = "shops:lists";
        public const string ShopsDetails = "shops:details";
    }

    // Tag -> redis patterns
    private static readonly Dictionary<string, string[]> TagPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        {
            Tags.ShopsDictionary,
            [
                "Shop:cities:*",
                "Shop:equipment:*",
                "Shop:beans:*",
                "Shop:roasters:*",
                "Shop:brewmethods:*"
            ]
        },
        {
            Tags.ShopsLists,
            [
                "Shop:city:*" // all paged lists by city
            ]
        },
        {
            Tags.ShopsDetails,
            [
                "Shop:id:*",
                "Shop:favorites:*"
            ]
        }
    };

    public async Task InvalidateAsync(CacheKey cacheKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cacheKey);
        await redisService.RemoveAsync(cacheKey);
    }

    public async Task InvalidateTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        foreach (var tag in tags)
        {
            if (!TagPatterns.TryGetValue(tag, out var patterns))
            {
                logger.LogDebug("No cache patterns registered for tag {Tag}", tag);
                continue;
            }

            foreach (var pattern in patterns)
            {
                await redisService.RemoveByPatternAsync(pattern);
            }
        }
    }
}

