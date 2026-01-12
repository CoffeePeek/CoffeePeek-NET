using CoffeePeek.Contract.Responses;
using Coffeepeek.Moderation.Application.Features.Review;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

public class SendReviewToModerationHandler(
    IValidationStrategy<SendReviewToModerationCommand> validationStrategy,
    IModerationReviewRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SendReviewToModerationCommand, CreateEntityResponse>
{
    public async Task<CreateEntityResponse> Handle(SendReviewToModerationCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);

        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }
        
        var review = ModerationReview.Create(request.UserId,
            request.ShopId,
            request.Header,
            request.Comment,
            request.RatingPlace,
            request.RatingService,
            request.RatingCoffee);

        repository.Add(review);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateEntityResponse.Success();
    }
}