using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Shops.Application.Commands.CoffeeShop;

public record GetCoffeeShopCommand(Guid Id) : IRequest<Response<GetCoffeeShopResponse>>;