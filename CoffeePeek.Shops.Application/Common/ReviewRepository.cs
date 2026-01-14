using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Entities.ReviewAggregate;

namespace CoffeePeek.Shops.Application.Common;

public class ReviewRepository(IGenericRepository<Review> reviewRepository) : IReviewRepository
{
    public async Task<Review?> GetByIdAsNoTracking(Guid reviewId, CancellationToken ct)
    {
        return await reviewRepository.FirstOrDefaultAsNoTrackingAsync(x => x.Id == reviewId, ct);
    }
    
    public async Task<Review?> GetById(Guid reviewId, CancellationToken ct)
    {
        return await reviewRepository.GetByIdAsync(reviewId, ct);
    }

    public void Update(Review review)
    {
        reviewRepository.Update(review);
    }

    public Task<bool> ExistsWithCurrentUser(Guid shopId, Guid userId, CancellationToken ct)
    {
        return reviewRepository.AnyAsync(x => x.ShopId == shopId && x.UserId == userId, ct);
    }
}