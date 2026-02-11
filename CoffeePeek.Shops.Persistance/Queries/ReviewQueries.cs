using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shops.Application.Features.Review;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Queries;

public class ReviewQueries(ShopsDbContext dbContext, IMapper mapper) : IReviewQueries
{
    private readonly DbSet<Review> _repository = dbContext.Reviews;
    
    public async Task<ReviewDto[]> GetReviewsByUserId(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await _repository.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ProjectToType<ReviewDto>(mapper.Config)
            .ToArrayAsync(ct);
    }
}