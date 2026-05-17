using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;

namespace CoffeePeek.Shops.Application.Services;

public class UserFavoriteService(
    IUserFavoriteRepository userFavoriteRepository,
    IQueryCoffeeShopRepository queryCoffeeShopRepository)
    : IUserFavoriteService
{
    public async Task<Guid> AddToFavoritesAsync(
        Guid userId,
        Guid coffeeShopId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("UserId cannot be empty");
        }

        if (coffeeShopId == Guid.Empty)
        {
            throw new ValidationException("CoffeeShopId cannot be empty");
        }

        var shopExists = await queryCoffeeShopRepository.Exists(coffeeShopId, ct);
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

        userFavoriteRepository.Add(favorite);

        return favorite.Id;
    }

    public async Task RemoveFromFavoritesAsync(
        Guid userId,
        Guid coffeeShopId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("UserId cannot be empty");
        }

        if (coffeeShopId == Guid.Empty)
        {
            throw new ValidationException("CoffeeShopId cannot be empty");
        }

        var favorite = await userFavoriteRepository.GetByUserAndShopAsync(userId, coffeeShopId, ct);
        if (favorite == null)
        {
            throw new NotFoundException("Coffee shop not found");
        }

        userFavoriteRepository.Remove(favorite);
    }
}