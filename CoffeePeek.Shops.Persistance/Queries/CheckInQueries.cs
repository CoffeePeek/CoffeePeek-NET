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
    
    public async Task<(CheckInDto[] Items, int TotalCount)> GetByUserId(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _repository.AsNoTracking().Where(x => x.UserId == userId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<CheckInDto>(mapper.Config)
            .ToArrayAsync(ct);

        return (items, totalCount);
    }
}