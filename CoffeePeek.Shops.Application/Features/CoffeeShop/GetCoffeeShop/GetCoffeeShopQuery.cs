using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;

public record GetCoffeeShopQuery(Guid Id, Guid? UserId = null) : IRequest<Response<GetCoffeeShopResponse>>;