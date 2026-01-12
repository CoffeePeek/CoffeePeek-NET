using CoffeePeek.Shared.Infrastructure.Cache;

namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface ICacheInvalidationStrategy
{
    Task InvalidateAsync(CacheKey cacheKey);
    
    Task InvalidateTagsAsync(IEnumerable<string> tags);
}