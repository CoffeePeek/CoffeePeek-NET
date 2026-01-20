using CoffeePeek.Contract.Abstract;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Validation;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Review.SendReviewToModeration;

public class SendReviewToModerationHandler(
    IAsyncValidationStrategy<SendReviewToModerationCommand> validationStrategy,
    IGenericRepository<ModerationShop> shopRepository,
    IGenericRepository<PhotoMetadata> photosRepository,
    IModerationReviewRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SendReviewToModerationCommand, CreateEntityResponse>
{
    public async Task<CreateEntityResponse> Handle(SendReviewToModerationCommand request,
        CancellationToken ct)
    {
        var validationResult = await validationStrategy.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }
        
        var moderationShop = await shopRepository.FirstOrDefaultAsNoTrackingAsync(x => x.ShopId == request.ShopId, ct);

        if (moderationShop == null)
        {
            throw new NotFoundException("Coffee shop not found");
        }
        
        var photos = new List<PhotoMetadata>();

        if (request.Photos != null)
        {
            photos = request.Photos.Select(x =>
                    PhotoMetadata.Create(x.FileName, x.ContentType, x.StorageKey, x.Size, request.UserId,
                        moderationShop.Id))
                .ToList();
            
            await photosRepository.AddRangeAsync(photos, ct);
        }
        
        var review = ModerationReview.Create(request.UserId,
            request.ShopId,
            moderationShopId:moderationShop.Id,
            request.UserName!,
            request.Header,
            request.Comment,
            request.RatingPlace,
            request.RatingService,                 
            request.RatingCoffee,
            photos);

        repository.Add(review);

        await unitOfWork.SaveChangesAsync(ct);

        return CreateEntityResponse.Success();
    }
}