using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetUserCheckInsCommand(Guid userId, int pageNumber, int pageSize)
    : IRequest<Response<GetUserCheckInsResponse>>
{
    public Guid UserId { get; init; } = userId;
    public int PageNumber { get; init; } = pageNumber;
    public int PageSize { get; init; } = pageSize;
}