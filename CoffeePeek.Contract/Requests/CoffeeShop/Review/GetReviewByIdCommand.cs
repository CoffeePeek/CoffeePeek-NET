using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public class GetReviewByIdCommand(Guid id) : IRequest<Response<GetReviewByIdResponse>>
{
    public Guid Id { get; } = id;
}