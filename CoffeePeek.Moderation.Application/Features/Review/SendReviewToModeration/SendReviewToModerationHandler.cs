using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;
using ModerationReview = CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate.ModerationReview;

namespace CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

public static class SendReviewToModerationHandler
{
    [Transactional]
    public static async Task<CreateEntityResponse> Handle(
        SendReviewToModerationCommand command,
        IQueryModerationShopRepository moderationShopRepository,
        IModerationReviewRepository repository,
        CancellationToken ct)
    {
        var moderationShop = await moderationShopRepository.GetById(command.ShopId, ct);

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
        
        return CreateEntityResponse.Success();
    }
}