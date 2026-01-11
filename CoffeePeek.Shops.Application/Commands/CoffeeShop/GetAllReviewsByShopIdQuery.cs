using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Shops.Application.Commands.CoffeeShop;

public record GetAllReviewsByShopIdQuery(Guid ShopId, int PageNumber = 1, int PageSize = 10)
    : IRequest<Response<GetAllReviewsResponse>>;
