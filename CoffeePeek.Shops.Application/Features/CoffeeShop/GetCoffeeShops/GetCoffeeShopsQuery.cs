using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shops.Application.Common.Responses;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShops;

public record GetCoffeeShopsQuery(Guid? UserId, Guid CityId, int PageNumber, int PageSize)
    : IRequest<Response<GetCoffeeShopsResponse>>;