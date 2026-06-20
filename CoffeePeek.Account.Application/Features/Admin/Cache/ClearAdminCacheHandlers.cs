using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Cache;

public record ClearAdminCacheByPatternCommand([Required, MinLength(3)] string Pattern);

public record ClearAdminCacheResponse(int ClearedCount, string Pattern);

public static class ClearAdminCacheByPatternHandler
{
    public static async Task<Response<ClearAdminCacheResponse>> Handle(
        ClearAdminCacheByPatternCommand command,
        ICacheService cacheService,
        CancellationToken ct)
    {
        if (!AdminCachePatternValidator.IsAllowed(command.Pattern))
            return Response<ClearAdminCacheResponse>.Error("Pattern is not allowed. FLUSHALL and wildcard-only patterns are forbidden.");

        var keys = await cacheService.GetKeysByPatternAsync(command.Pattern, 10_000, ct);
        await cacheService.RemoveByPattern(command.Pattern, ct);

        return Response<ClearAdminCacheResponse>.Success(new ClearAdminCacheResponse(keys.Count, command.Pattern));
    }
}

public record ClearAdminCacheKeyCommand([Required, MinLength(3)] string Key);

public static class ClearAdminCacheKeyHandler
{
    public static async Task<Response> Handle(
        ClearAdminCacheKeyCommand command,
        ICacheService cacheService,
        CancellationToken ct)
    {
        if (!AdminCachePatternValidator.IsAllowedKey(command.Key))
            return Response.Error("Key is not allowed.");

        await cacheService.RemoveAsync(new CacheKey(command.Key));
        return Response.Success(message: "Cache key removed.");
    }
}

internal static class AdminCachePatternValidator
{
    public static bool IsAllowed(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        if (pattern is "*" or "**")
            return false;

        return pattern.Contains(':');
    }

    public static bool IsAllowedKey(string key) =>
        !string.IsNullOrWhiteSpace(key) && key.Contains(':') && key != "*";
}
