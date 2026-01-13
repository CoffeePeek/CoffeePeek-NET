using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;

public class AddToFavoriteHandler(
    IUserFavoriteService userFavoriteService,
    IValidationStrategy<AddToFavoriteCommand> validationStrategy)
    : IRequestHandler<AddToFavoriteCommand, CreateEntityResponse<Guid>>
{
    public async Task<CreateEntityResponse<Guid>> Handle(AddToFavoriteCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }

        var res = await userFavoriteService.AddToFavoritesAsync(request.UserId, request.CoffeeShopId,
            cancellationToken);

        return res.IsSuccess
            ? CreateEntityResponse<Guid>.Success(res.Data, "Coffee shop added to favorites")
            : CreateEntityResponse<Guid>.Error(res.ErrorMessage);
    }
}