using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Services;

public class UserVisitService(
    IGenericRepository<UserVisit> visitRepository,
    IUserVisitRepository userVisitRepository,
    IGenericRepository<CoffeeShop> shopRepository,
    ICoffeeShopRepository coffeeShopRepository,
    IUnitOfWork unitOfWork,
    IRedisService redisService)
    : IUserVisitService
{
    public async Task<Result<Guid>> RegisterVisitAsync(
        Guid userId,
        Guid shopId,
        DateTime visitedAt,
        bool hasReview = false,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return Result<Guid>.Failure("UserId cannot be empty");

        if (shopId == Guid.Empty)
            return Result<Guid>.Failure("ShopId cannot be empty");

        var shopExists = await coffeeShopRepository.Exists(shopId, ct);
        if (!shopExists)
            return Result<Guid>.Failure("Coffee shop not found");

        var existingVisit = await userVisitRepository.GetByUserAndShopAsync(userId, shopId, ct);

        if (existingVisit == null)
        {
            var newVisit = UserVisit.CreateFirstVisit(userId, shopId, visitedAt);

            if (hasReview)
                newVisit.MarkAsReviewed();

            await visitRepository.AddAsync(newVisit, ct);
            await unitOfWork.SaveChangesAsync(ct);

            await InvalidateVisitedCache(userId);

            return Result<Guid>.Success(newVisit.Id);
        }

        existingVisit.RegisterVisit(visitedAt);

        if (hasReview && !existingVisit.HasReview)
            existingVisit.MarkAsReviewed();

        visitRepository.Update(existingVisit);
        await unitOfWork.SaveChangesAsync(ct);

        await InvalidateVisitedCache(userId);

        return Result<Guid>.Success(existingVisit.Id);
    }

    public async Task<Result> UpdateReviewStatusAsync(
        Guid userId,
        Guid shopId,
        bool hasReview,
        CancellationToken ct = default)
    {
        var visit = await userVisitRepository.GetByUserAndShopAsync(userId, shopId, ct);

        if (visit == null)
            return Result.Failure("Visit not found");

        if (hasReview)
            visit.MarkAsReviewed();

        visitRepository.Update(visit);
        await unitOfWork.SaveChangesAsync(ct);

        await InvalidateVisitedCache(userId);

        return Result.Success();
    }

    public async Task<List<CoffeeShop>> GetVisitedShopsAsync(
        Guid userId,
        VisitedSortOrder sortOrder = VisitedSortOrder.LastVisited,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return [];

        var visitsQuery = visitRepository
            .Query()
            .Where(v => v.UserId == userId);

        visitsQuery = sortOrder switch
        {
            VisitedSortOrder.FirstVisited => visitsQuery.OrderBy(v => v.FirstVisitedAt),
            VisitedSortOrder.LastVisited => visitsQuery.OrderByDescending(v => v.LastVisitedAt),
            VisitedSortOrder.MostVisited => visitsQuery.OrderByDescending(v => v.VisitCount),
            _ => visitsQuery.OrderByDescending(v => v.LastVisitedAt)
        };

        var visits = await visitsQuery.ToListAsync(ct);
        var shopIds = visits.Select(v => v.ShopId).ToList();

        if (shopIds.Count == 0)
            return [];

        var shops = await shopRepository
            .Query()
            .Where(s => shopIds.Contains(s.Id))
            .Include(s => s.ShopPhotos)
            .Include(s => s.Reviews)
            .Include(s => s.Schedules)
            .ToListAsync(ct);

        if (sortOrder == VisitedSortOrder.Alphabetical)
        {
            return shops.OrderBy(s => s.Name).ToList();
        }

        return shopIds
            .Select(id => shops.FirstOrDefault(s => s.Id == id))
            .Where(s => s != null)
            .Cast<CoffeeShop>()
            .ToList();
    }

    public async Task<UserVisitStatistics> GetVisitStatisticsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return new UserVisitStatistics();

        var visits = await visitRepository
            .Query()
            .Where(v => v.UserId == userId)
            .ToListAsync(ct);

        if (visits.Count == 0)
            return new UserVisitStatistics();

        var favoriteShop = visits
            .OrderByDescending(v => v.VisitCount)
            .ThenByDescending(v => v.LastVisitedAt)
            .FirstOrDefault();

        return new UserVisitStatistics
        {
            UniqueShopsVisited = visits.Count,
            TotalCheckIns = visits.Sum(v => v.VisitCount),
            ShopsWithReviews = visits.Count(v => v.HasReview),
            FavoriteShopId = favoriteShop?.ShopId,
            FavoriteShopVisitCount = favoriteShop?.VisitCount ?? 0
        };
    }

    public async Task<bool> HasVisitedAsync(Guid userId, Guid shopId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty || shopId == Guid.Empty)
            return false;

        return await userVisitRepository.ExistsAsync(userId, shopId, ct);
    }

    private async Task InvalidateVisitedCache(Guid userId)
    {
        await redisService.RemoveAsync(CacheKey.Shop.Visited(userId));
        await redisService.RemoveAsync(CacheKey.Shop.VisitStatistics(userId));
    }
}
