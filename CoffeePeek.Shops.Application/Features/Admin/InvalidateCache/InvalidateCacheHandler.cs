using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Application.Features.Admin.InvalidateCache;

public class InvalidateCacheHandler(
    ICacheInvalidationStrategy cacheInvalidationStrategy,
    IRedisService redisService,
    ILogger<InvalidateCacheHandler> logger) 
    : IRequestHandler<InvalidateCacheCommand, Response<InvalidateCacheResponse>>
{
    private static readonly Dictionary<string, string> CategoryToTag = new(StringComparer.OrdinalIgnoreCase)
    {
        { CacheKey.Categories.Shops.Dictionaries, CacheInvalidationStrategy.Tags.ShopsDictionary },
        { CacheKey.Categories.Shops.Lists, CacheInvalidationStrategy.Tags.ShopsLists },
        { CacheKey.Categories.Shops.Details, CacheInvalidationStrategy.Tags.ShopsDetails }
    };

    public async Task<Response<InvalidateCacheResponse>> Handle(
        InvalidateCacheCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.InvalidateAll)
            {
                // Invalidate all cache by pattern
                await redisService.RemoveByPattern("*");
                
                logger.LogInformation("Admin: All cache invalidated");
                
                return Response<InvalidateCacheResponse>.Success(
                    new InvalidateCacheResponse(
                        "All cache successfully invalidated",
                        Category: "all"));
            }

            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return Response<InvalidateCacheResponse>.Error(
                    "Category is required when InvalidateAll is false. " +
                    $"Available categories: {string.Join(", ", CategoryToTag.Keys)}");
            }

            if (!CategoryToTag.TryGetValue(request.Category, out var tag))
            {
                return Response<InvalidateCacheResponse>.Error(
                    $"Invalid category '{request.Category}'. " +
                    $"Available categories: {string.Join(", ", CategoryToTag.Keys)}");
            }

            await cacheInvalidationStrategy.InvalidateTagsAsync([tag]);
            
            logger.LogInformation("Admin: Cache category '{Category}' invalidated (tag: {Tag})", 
                request.Category, tag);

            return Response<InvalidateCacheResponse>.Success(
                new InvalidateCacheResponse(
                    $"Cache category '{request.Category}' successfully invalidated",
                    Category: request.Category));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to invalidate cache. Category: {Category}, InvalidateAll: {InvalidateAll}", 
                request.Category, request.InvalidateAll);
            
            return Response<InvalidateCacheResponse>.Error(
                "Failed to invalidate cache. See logs for details.");
        }
    }
}
