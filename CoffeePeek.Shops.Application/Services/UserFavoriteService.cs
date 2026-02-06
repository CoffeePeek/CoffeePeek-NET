using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;

namespace CoffeePeek.Shops.Application.Services;

public class UserFavoriteService(
    IGenericRepository<UserFavorite> favoriteRepository,
    IUserFavoriteRepository userFavoriteRepository,
    ICoffeeShopRepository coffeeShopRepository,
    IUnitOfWork unitOfWork)
    : IUserFavoriteService
{
    public async Task<Guid> AddToFavoritesAsync(
        Guid userId,
        Guid coffeeShopId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException("UserId cannot be empty");
        }

        if (coffeeShopId == Guid.Empty)
        {
            throw new InvalidOperationException("CoffeeShopId cannot be empty");
        }

        var shopExists = await coffeeShopRepository.Exists(coffeeShopId, ct);
        if (!shopExists)
        {
            throw new NotFoundException("Coffee shop not found");
        }

        var alreadyExists = await userFavoriteRepository.Exists(userId, coffeeShopId, ct);
        if (alreadyExists)
        {
            throw new ConflictException("Coffee shop is already in favorites");
        }

        var favorite = UserFavorite.Create(userId, coffeeShopId);

        await favoriteRepository.AddAsync(favorite, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return favorite.Id;
    }

    public async Task RemoveFromFavoritesAsync(
        Guid userId,
        Guid coffeeShopId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException("UserId cannot be empty");
        }

        if (coffeeShopId == Guid.Empty)
        {
            throw new InvalidOperationException("CoffeeShopId cannot be empty");
        }

        var favorite = await userFavoriteRepository.GetByUserAndShopAsync(userId, coffeeShopId, ct);
        if (favorite == null)
        {
            throw new NotFoundException("Coffee shop not found");
        }

        favoriteRepository.Remove(favorite);
        await unitOfWork.SaveChangesAsync(ct);
    }
}