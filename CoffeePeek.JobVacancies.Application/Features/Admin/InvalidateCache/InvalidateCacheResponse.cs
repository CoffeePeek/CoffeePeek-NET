namespace CoffeePeek.JobVacancies.Application.Features.Admin.InvalidateCache;

public record InvalidateCacheResponse(
    string Message,
    int InvalidatedKeysCount = 0);
