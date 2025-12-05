using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record GetCoffeeShopCommand(Guid Id) : IRequest<Response.Response<GetCoffeeShopResponse>>;