using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.JobVacancies.Application.Features.Admin.InvalidateCache;

public class InvalidateCacheHandler(
    IRedisService redisService,
    ILogger<InvalidateCacheHandler> logger) 
    : IRequestHandler<InvalidateCacheCommand, Response<InvalidateCacheResponse>>
{
    public async Task<Response<InvalidateCacheResponse>> Handle(
        InvalidateCacheCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            await redisService.RemoveByPatternAsync(CacheKey.Vacancy.AllPattern());
            
            logger.LogInformation("Admin: All vacancies cache invalidated");
            
            return Response<InvalidateCacheResponse>.Success(
                new InvalidateCacheResponse("All vacancies cache successfully invalidated"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to invalidate vacancies cache");
            
            return Response<InvalidateCacheResponse>.Error(
                "Failed to invalidate cache. See logs for details.");
        }
    }
}
