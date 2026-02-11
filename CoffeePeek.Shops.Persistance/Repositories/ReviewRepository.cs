using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class ReviewRepository(ShopsDbContext dbContext) : IReviewRepository
{
    private readonly DbSet<Review> _repository = dbContext.Reviews;
    
    public void Add(Review review)
    {
        _repository.Add(review);
    }
    
    public Task<bool> AnyAsync(Guid shopId, Guid reviewId, CancellationToken ct)
    {
        return _repository.AnyAsync(r => r.CoffeeShopId == shopId && r.Id == reviewId, ct);
    }
    
    public Task<Review?> GetById(Guid reviewId, CancellationToken ct)
    {
        return _repository.FirstOrDefaultAsync(x => x.Id == reviewId, ct);
    }

    public Task<Review[]> GetByUserId(Guid userId, CancellationToken ct)
    {
        return _repository.Where(x => x.UserId == userId).OrderBy(x => x.CreatedAtUtc).ToArrayAsync(ct);
    }
}