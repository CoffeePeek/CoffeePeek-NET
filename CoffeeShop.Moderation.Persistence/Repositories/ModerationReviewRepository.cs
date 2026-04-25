using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeeShop.Moderation.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using ModerationReview = CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate.ModerationReview;

namespace CoffeeShop.Moderation.Persistence.Repositories;

public class ModerationReviewRepository(ModerationDbContext dbContext) : IModerationReviewRepository
{
    private readonly DbSet<ModerationReview> _reviewRepository = dbContext.ModerationReviews;
    
    public void Add(ModerationReview review)
    {
        _reviewRepository.Add(review);
    }
    
    public Task<ModerationReview?> GetById(Guid id, CancellationToken ct = default)
    {
        return _reviewRepository.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<ModerationReview?> GetByShopId(Guid shopId, CancellationToken ct = default)
    {
        return _reviewRepository.FirstOrDefaultAsync(x => x.ShopId == shopId, ct);
    }
}