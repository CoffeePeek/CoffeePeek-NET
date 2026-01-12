using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain.Entities.ModerationReviewAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Coffeepeek.Moderation.Application.Common;

public class ModerationReviewRepository(IGenericRepository<ModerationReview> reviewRepository)
    : IModerationReviewRepository
{
    public Task<ModerationReview[]> GetAllPending()
    {
        return reviewRepository
            .QueryAsNoTracking()
            .Where(x => x.ModerationStatus == ModerationStatus.Pending)
            .ToArrayAsync();
    }

    public Task<ModerationReview?> GetById(Guid id)
    {
        return reviewRepository
            .Query()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<ModerationReview?> GetByShopId(Guid shopId)
    {
        return reviewRepository
            .Query()
            .FirstOrDefaultAsync(x => x.ShopId == shopId);
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