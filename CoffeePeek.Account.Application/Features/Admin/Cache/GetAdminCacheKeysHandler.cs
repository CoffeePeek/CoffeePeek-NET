using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Admin.Cache;

public record GetAdminCacheKeysQuery(
    [Required, MinLength(3)] string Pattern,
    [Range(1, 1000)] int Limit = 100);

public record GetAdminCacheKeysResponse(IReadOnlyList<string> Keys, int Count);

public static class GetAdminCacheKeysHandler
{
    public static async Task<Response<GetAdminCacheKeysResponse>> Handle(
        GetAdminCacheKeysQuery query,
        ICacheService cacheService,
        CancellationToken ct)
    {
        if (!AdminCachePatternValidator.IsAllowed(query.Pattern))
            return Response<GetAdminCacheKeysResponse>.Error("Pattern is not allowed. Use a scoped prefix like user:* or shop:*.");

        var keys = await cacheService.GetKeysByPatternAsync(query.Pattern, query.Limit, ct);
        return Response<GetAdminCacheKeysResponse>.Success(new GetAdminCacheKeysResponse(keys, keys.Count));
    }
}
