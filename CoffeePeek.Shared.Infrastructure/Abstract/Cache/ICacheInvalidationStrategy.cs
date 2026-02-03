using CoffeePeek.Shared.Infrastructure.Persistence;

namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface ICacheInvalidationStrategy
{
    Task InvalidateAsync(CacheKey cacheKey);
    
    Task InvalidateTagsAsync(IEnumerable<string> tags);
}