using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Favorite.RemoveFromFavorite;

public class RemoveFromFavoriteHandler(
    IUserFavoriteService favoriteService,
    IValidationStrategy<RemoveFromFavoriteCommand> validationStrategy)
    : IRequestHandler<RemoveFromFavoriteCommand, UpdateEntityResponse<Guid>>
{
    public async Task<UpdateEntityResponse<Guid>> Handle(RemoveFromFavoriteCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return UpdateEntityResponse<Guid>.Error(validationResult.ErrorMessage);
        }

        await favoriteService.RemoveFromFavoritesAsync(request.UserId, request.CoffeeShopId,
            cancellationToken);

        return UpdateEntityResponse<Guid>.Success(request.CoffeeShopId, "Coffee shop removed from favorites");
    }
}