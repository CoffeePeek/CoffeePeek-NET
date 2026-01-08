using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

public record GetCoffeeShopQuery(Guid Id) : IRequest<Response<GetCoffeeShopResponse>>;