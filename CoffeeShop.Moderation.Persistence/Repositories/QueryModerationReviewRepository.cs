using CoffeePeek.Moderation.Domain.Aggregates.ModerationReviewAggregate;
using CoffeeShop.Moderation.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Moderation.Persistence.Repositories;

public class QueryModerationReviewRepository(ModerationDbContext dbContext) : IQueryModerationReviewRepository
{
    private readonly DbSet<ModerationReview> _reviewRepository = dbContext.ModerationReviews;
    
    public Task<ModerationReview[]> GetAll(CancellationToken ct = default)
    {
        return _reviewRepository.AsNoTracking().ToArrayAsync(ct);
    }
}