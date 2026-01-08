namespace CoffeePeek.Shops.Application.Features.Admin.InvalidateCache;

public record InvalidateCacheResponse(
    string Message,
    string? Category = null,
    int InvalidatedKeysCount = 0);
