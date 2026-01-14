using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.Review.GetReviewById;

public class GetReviewByIdQuery(Guid id) : IRequest<Response<GetReviewByIdResponse>>
{
    public Guid Id { get; } = id;
}