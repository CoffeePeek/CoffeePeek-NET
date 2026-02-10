namespace CoffeePeek.Account.Application.Features.Admin.InvalidateCache;

public record InvalidateCacheCommand(string? Category = null, bool InvalidateAll = false);
