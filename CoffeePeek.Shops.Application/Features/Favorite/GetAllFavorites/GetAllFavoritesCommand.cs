using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Favorite.GetAllFavorites;

public record GetAllFavoritesCommand(Guid UserId) : IRequest<Response<GetAllFavoritesResponse>>;