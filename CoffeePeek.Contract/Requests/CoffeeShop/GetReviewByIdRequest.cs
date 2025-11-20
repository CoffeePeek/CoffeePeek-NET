using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class GetReviewByIdRequest(int id) : IRequest<Response<GetReviewByIdResponse>>
{
    public int Id { get; } = id;
}