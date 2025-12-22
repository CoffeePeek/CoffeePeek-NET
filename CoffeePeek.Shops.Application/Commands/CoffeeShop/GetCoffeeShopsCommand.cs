using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record GetCoffeeShopsCommand(Guid CityId, int PageNumber, int PageSize)
    : IRequest<Response<GetCoffeeShopsResponse>>
{
    public Guid CityId { get; } = CityId;
    public int PageNumber { get; } = PageNumber;
    public int PageSize { get; } = PageSize;
}