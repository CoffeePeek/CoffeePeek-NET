using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;

namespace CoffeePeek.Shared.Persistence.Cache;

public interface ICacheInvalidationStrategy
{
    Task InvalidateAsync(CacheKey cacheKey);
    
    Task InvalidateTagsAsync(IEnumerable<string> tags);
}