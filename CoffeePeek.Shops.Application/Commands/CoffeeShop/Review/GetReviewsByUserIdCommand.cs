using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public record GetReviewsByUserIdCommand(
    Guid UserId,
    [Range(1, int.MaxValue)] int PageNumber = 1,
    [Range(1, 100)] int PageSize = 10)
    : IRequest<Response<GetReviewsByUserIdResponse>>;