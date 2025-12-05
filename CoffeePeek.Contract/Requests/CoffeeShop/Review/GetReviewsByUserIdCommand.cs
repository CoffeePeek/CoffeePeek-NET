using CoffeePeek.Contract.Response.CoffeeShop.Review;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public record GetReviewsByUserIdCommand(Guid UserId) : IRequest<Response.Response<GetReviewsByUserIdResponse>>;