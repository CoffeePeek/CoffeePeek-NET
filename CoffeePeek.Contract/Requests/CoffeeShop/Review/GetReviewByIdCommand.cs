using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public class GetReviewByIdCommand(Guid id) : IRequest<Response<GetReviewByIdResponse>>
{
    public Guid Id { get; } = id;
}