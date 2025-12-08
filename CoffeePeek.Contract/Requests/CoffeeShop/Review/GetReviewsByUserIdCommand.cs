using CoffeePeek.Contract.Response.CoffeeShop.Review;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public record GetReviewsByUserIdCommand(Guid UserId, int PageNumber = 1, int PageSize = 10)
    : IRequest<Response.Response<GetReviewsByUserIdResponse>>;