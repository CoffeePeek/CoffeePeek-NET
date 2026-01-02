using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop.Review;
using MediatR;

namespace CoffeePeek.Shops.Application.Commands.CoffeeShop.Review;

public class GetReviewByIdCommand(Guid id) : IRequest<Response<GetReviewByIdResponse>>
{
    public Guid Id { get; } = id;
}