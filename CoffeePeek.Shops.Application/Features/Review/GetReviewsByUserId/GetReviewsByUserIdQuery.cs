using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.GetReviewsByUserId;

public record GetReviewsByUserIdQuery(
    Guid UserId,
    [Range(1, int.MaxValue)] int PageNumber = 1,
    [Range(1, 100)] int PageSize = 10)
    : IRequest<Response<GetReviewsByUserIdResponse>>;