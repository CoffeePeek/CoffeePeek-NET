using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Moderation.Application.Common;

public class ModerationReviewRepository(IGenericRepository<ModerationReview> reviewRepository)
    : IModerationReviewRepository
{
    public Task<ModerationReview[]> GetAll(CancellationToken ct = default)
    {
        return reviewRepository
            .QueryAsNoTracking()
            .ToArrayAsync(ct);
    }

    public Task<ModerationReview?> GetById(Guid id, CancellationToken ct = default)
    {
        return reviewRepository
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<ModerationReview?> GetByShopId(Guid shopId, CancellationToken ct = default)
    {
        return reviewRepository
            .Query()
            .FirstOrDefaultAsync(x => x.ShopId == shopId, ct);
    }

    public void Add(ModerationReview review)
    {
        reviewRepository.Add(review);
    }

    public void Update(ModerationReview review)
    {
        reviewRepository.Update(review);
    }
}