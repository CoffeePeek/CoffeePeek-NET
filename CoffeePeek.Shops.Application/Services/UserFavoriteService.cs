using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Services;

public class UserFavoriteService(
    IGenericRepository<UserFavorite> favoriteRepository,
    IUserFavoriteRepository userFavoriteRepository,
    IGenericRepository<CoffeeShop> shopRepository,
    ICoffeeShopRepository coffeeShopRepository,
    IUnitOfWork unitOfWork,
    IRedisService redisService)
    : IUserFavoriteService
{
    public async Task<Result<Guid>> AddToFavoritesAsync(
        Guid userId,
        Guid coffeeShopId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return Result<Guid>.Failure("UserId cannot be empty");

        if (coffeeShopId == Guid.Empty)
            return Result<Guid>.Failure("CoffeeShopId cannot be empty");

        var shopExists = await coffeeShopRepository.Exists(coffeeShopId, ct);
        if (!shopExists)
            return Result<Guid>.Failure("Coffee shop not found");

        var alreadyExists = await userFavoriteRepository.ExistsAsync(userId, coffeeShopId, ct);
        if (alreadyExists)
            return Result<Guid>.Failure("Coffee shop is already in favorites");

        var favorite = UserFavorite.Create(userId, coffeeShopId);

        await favoriteRepository.AddAsync(favorite, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await InvalidateFavoritesCache(userId);

        return Result<Guid>.Success(favorite.Id);
    }

    public async Task<Result> RemoveFromFavoritesAsync(
        Guid userId,
        Guid coffeeShopId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return Result.Failure("UserId cannot be empty");

        if (coffeeShopId == Guid.Empty)
            return Result.Failure("CoffeeShopId cannot be empty");

        var favorite = await userFavoriteRepository.GetByUserAndShopAsync(userId, coffeeShopId, ct);
        if (favorite == null)
            return Result.Failure("Coffee shop is not in favorites");

        favoriteRepository.Remove(favorite);
        await unitOfWork.SaveChangesAsync(ct);

        await InvalidateFavoritesCache(userId);

        return Result.Success();
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid coffeeShopId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty || coffeeShopId == Guid.Empty)
            return false;

        return await userFavoriteRepository.ExistsAsync(userId, coffeeShopId, ct);
    }

    public async Task<List<CoffeeShop>> GetUserFavoritesAsync(Guid userId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return [];

        var favoriteShopIds = await userFavoriteRepository.GetFavoriteShopIdsAsync(userId, ct);

        if (favoriteShopIds.Count == 0)
            return [];

        var shops = await shopRepository
            .Query()
            .Where(s => favoriteShopIds.Contains(s.Id))
            .Include(s => s.ShopPhotos)
            .Include(s => s.Reviews)
            .Include(s => s.Schedules)
            .ToListAsync(ct);

        return favoriteShopIds
            .Select(id => shops.FirstOrDefault(s => s.Id == id))
            .Where(s => s != null)
            .Cast<Domain.Entities.CoffeeShopAggregate.CoffeeShop>()
            .ToList();
    }

    public async Task<int> GetFavoritesCountAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return 0;

        return await userFavoriteRepository.GetFavoritesCountAsync(userId, ct);
    }

    private async Task InvalidateFavoritesCache(Guid userId)
    {
        await redisService.RemoveAsync(CacheKey.Shop.Favorites(userId));
    }
}