using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record GetCoffeeShopCommand(Guid Id) : IRequest<Response<GetCoffeeShopResponse>>;