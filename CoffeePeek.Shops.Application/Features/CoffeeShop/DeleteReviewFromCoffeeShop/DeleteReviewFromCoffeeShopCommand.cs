using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.DeleteReviewFromCoffeeShop;

public record DeleteReviewFromCoffeeShopCommand(Guid CoffeeShopId) : IRequest<Response>;