using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using MediatR;

namespace CoffeePeek.Shops.Application.Commands.CoffeeShop;

public record GetCoffeeShopsCommand(Guid CityId, int PageNumber, int PageSize)
    : IRequest<Response<GetCoffeeShopsResponse>>
{
    public Guid CityId { get; } = CityId;
    public int PageNumber { get; } = PageNumber;
    public int PageSize { get; } = PageSize;
}