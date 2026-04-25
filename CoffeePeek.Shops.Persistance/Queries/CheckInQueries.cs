using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shops.Application.Features.CheckIn;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Persistance.Queries;

public class CheckInQueries(ShopsDbContext dbContext, IMapper mapper) : ICheckInQueries
{
    private readonly DbSet<CheckIn> _repository = dbContext.CheckIns;
    
    public async Task<CheckInDto[]> GetByUserId(Guid userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await _repository.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageNumber)
            .ProjectToType<CheckInDto>(mapper.Config)
            .ToArrayAsync(ct);
    }
}