namespace CoffeePeek.Shared.Infrastructure.Cache;

public interface ICacheInvalidationStrategy
{
    Task InvalidateAsync(CacheKey cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate grouped cache entries by tag (translated into patterns).
    /// </summary>
    Task InvalidateTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
}