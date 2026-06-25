using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shared.Validation;
using ModerationReview = CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate.ModerationReview;

namespace CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

public static class SendReviewToModerationHandler
{
    public static async Task<CreateEntityResponse> Handle(
        SendReviewToModerationCommand command,
        IAsyncValidationStrategy<SendReviewToModerationCommand> validationStrategy,
        IQueryModerationShopRepository moderationShopRepository,
        IModerationReviewRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var validationResult = await validationStrategy.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.ErrorMessage!);

        var moderationShop = await moderationShopRepository.GetByPublishedShopId(command.ShopId, ct);

        if (moderationShop == null)
            throw new NotFoundException("Coffee shop not found");

        var photos = new List<PhotoMetadata>();
        if (command.Photos != null && command.Photos.Count != 0)
        {
            photos = command.Photos
                .Select(x => PhotoMetadata.Create(
                    x.FileName, 
                    x.ContentType, 
                    x.StorageKey, 
                    x.Size, 
                    command.UserId, 
                    moderationShop.Id))
                .ToList();
        }

        var review = ModerationReview.Create(
            command.UserId,
            command.ShopId,
            moderationShop.Id,
            command.UserName,
            command.Header,
            command.Comment,
            command.Rating.Place, command.Rating.Service, command.Rating.Coffee,
            photos);

        repository.Add(review);
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return CreateEntityResponse.Success();
    }
}