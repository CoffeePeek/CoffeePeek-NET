using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Shops.Application.Features.Review.GetReviewsByUserId;

public class GetReviewsByUserIdRequestHandler
{
    public async Task<Response<GetReviewsByUserIdResponse>> Handle(GetReviewsByUserIdQuery request,
        IReviewQueries reviewQueries,
        CancellationToken cancellationToken)
    {
        var reviewDtos = await reviewQueries.GetReviewsByUserId(request.UserId, request.PageNumber, request.PageSize,
            cancellationToken);

        var response = new GetReviewsByUserIdResponse(
            reviewDtos,
            TotalItems: reviewDtos.Length,
            TotalPages: (int)Math.Ceiling(reviewDtos.Length / (double)request.PageSize),
            request.PageNumber,
            request.PageSize);

        return Response<GetReviewsByUserIdResponse>.Success(response);
    }
}