using CoffeePeek.Contract.Dtos.Internal;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shops.Application.Services;
using MediatR;

namespace CoffeePeek.Shops.Application.Handlers.CoffeeShop.CheckIn;

public class CreateCheckInHandler(
    IGenericRepository<Domain.Entities.CheckIn> checkInRepository,
    IGenericRepository<Domain.Entities.Review> reviewsRepository,
    IGenericRepository<Domain.Entities.Shop> shopRepository,
    IUnitOfWork unitOfWork,
    IValidationStrategy<CreateCheckInRequest> validationStrategy,
    IHybridCache hybridCache,
    IRedisService redisService,
    IOutboxEventPublisher outboxEventPublisher)
    : IRequestHandler<CreateCheckInRequest, Contract.Responses.Response<CreateCheckInResponse>>
{
    public async Task<Contract.Responses.Response<CreateCheckInResponse>> Handle(CreateCheckInRequest request, CancellationToken cancellationToken)
    {
        var validationResult = validationStrategy.Validate(request);
        if (!validationResult.IsValid)
        {
            return Contract.Responses.Response<CreateCheckInResponse>.Error(validationResult.ErrorMessage);
        }

        var checkIn = new Domain.Entities.CheckIn
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ShopId = request.ShopId,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        Guid? reviewId = null;

        if (request.Review != null)
        {
            var review = new Domain.Entities.Review
            {
                Id = Guid.NewGuid(),
                Header = request.Review.Header,
                Comment = request.Review.Comment,
                UserId = request.UserId,
                ShopId = request.ShopId,
                RatingCoffee = request.Review.RatingCoffee ?? 0,
                RatingPlace = request.Review.RatingPlace ?? 0,
                RatingService = request.Review.RatingService ?? 0,
                ReviewDate = DateTime.UtcNow
            };

            reviewsRepository.Add(review);
            checkIn.ReviewId = review.Id;
            reviewId = review.Id;
        }

        checkInRepository.Add(checkIn);
        
        if (request.Review != null)
        {
            await InvalidateShopCacheAsync(request.ShopId, cancellationToken);
        }
        
        await outboxEventPublisher.PublishAsync(new CheckinCreatedEvent
        {
            UserId = request.UserId,
            ShopId = request.ShopId,
            CreatedAt = checkIn.CreatedAt
        }, cancellationToken);

        if (reviewId.HasValue)
        {
            await outboxEventPublisher.PublishAsync(new ReviewAddedEvent
            {
                UserId = request.UserId,
                ShopId = request.ShopId,
                ReviewId = reviewId.Value,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Contract.Responses.Response<CreateCheckInResponse>.Success(new CreateCheckInResponse(checkIn.Id, reviewId));
    }

    private async Task InvalidateShopCacheAsync(Guid shopId, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetByIdAsync(shopId, cancellationToken);
        if (shop == null) return;
        
        await hybridCache.RemoveAsync(CacheKey.Shop.Detail(shopId), cancellationToken);
        
        if (shop.CityId != Guid.Empty)
        {
            await redisService.RemoveByPatternAsync(CacheKey.Shop.ListByCityPattern(shop.CityId));
        }
    }
}