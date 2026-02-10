using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Features.Admin.InvalidateCache;

public class InvalidateCacheHandler
{
    private static readonly Dictionary<string, string[]> CategoryToPatterns =
        CacheKey.Categories.Account.GetPatterns();

    public static async Task<Response<InvalidateCacheResponse>> Handle(
        InvalidateCacheCommand request,
        ICacheService redisService,
        ILogger<InvalidateCacheHandler> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.InvalidateAll)
            {
                await redisService.RemoveByPattern("*");

                logger.LogInformation("Admin: All cache invalidated in AccountService");

                return Response<InvalidateCacheResponse>.Success(
                    new InvalidateCacheResponse(
                        "All cache successfully invalidated",
                        Category: "all"));
            }

            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return Response<InvalidateCacheResponse>.Error(
                    "Category is required when InvalidateAll is false. " +
                    $"Available categories: {string.Join(", ", CategoryToPatterns.Keys)}");
            }

            if (!CategoryToPatterns.TryGetValue(request.Category, out var patterns))
            {
                return Response<InvalidateCacheResponse>.Error(
                    $"Invalid category '{request.Category}'. " +
                    $"Available categories: {string.Join(", ", CategoryToPatterns.Keys)}");
            }

            foreach (var pattern in patterns)
            {
                await redisService.RemoveByPattern(pattern);
            }

            logger.LogInformation(
                "Admin: Cache category '{Category}' invalidated in AccountService (patterns: {Patterns})",
                request.Category, string.Join(", ", patterns));

            return Response<InvalidateCacheResponse>.Success(
                new InvalidateCacheResponse(
                    $"Cache category '{request.Category}' successfully invalidated",
                    Category: request.Category));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to invalidate cache in AccountService. Category: {Category}, InvalidateAll: {InvalidateAll}",
                request.Category, request.InvalidateAll);

            return Response<InvalidateCacheResponse>.Error(
                "Failed to invalidate cache. See logs for details.");
        }
    }
}