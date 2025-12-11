using CoffeePeek.JobVacancies.Configuration;
using CoffeePeek.JobVacancies.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.JobVacancies.Repository;

public class CityRepository(JobVacanciesDbContext dbContext) : ICityRepository
{
    private readonly DbSet<CityMap> _cities = dbContext.Cities;

    public async Task<IReadOnlyList<int>> GetHhAreasAsync(CancellationToken cancellationToken = default)
    {
        return await _cities.AsNoTracking().Select(x => x.HhAreaId).ToArrayAsync(cancellationToken);
    }

    public async Task UpsertAsync(CityMap map, CancellationToken cancellationToken = default)
    {
        var existing = await _cities.FirstOrDefaultAsync(x => x.CityId == map.CityId, cancellationToken);
        if (existing is null)
        {
            if (map.Id == Guid.Empty)
            {
                map.Id = Guid.NewGuid();
            }
            map.UpdatedAt = DateTime.UtcNow;
            _cities.Add(map);
        }
        else
        {
            existing.HhAreaId = map.HhAreaId;
            existing.UpdatedAt = DateTime.UtcNow;
            _cities.Update(existing);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CityMap?> GetByHhAreaIdAsync(int hhAreaId, CancellationToken cancellationToken = default)
    {
        return await _cities.AsNoTracking()
            .FirstOrDefaultAsync(x => x.HhAreaId == hhAreaId, cancellationToken);
    }

    public async Task<Dictionary<int, CityMap>> GetByHhAreaIdsAsync(IEnumerable<int> hhAreaIds, CancellationToken cancellationToken = default)
    {
        var areaIdsList = hhAreaIds.ToList();
        if (areaIdsList.Count == 0)
        {
            return new Dictionary<int, CityMap>();
        }

        var cities = await _cities.AsNoTracking()
            .Where(x => areaIdsList.Contains(x.HhAreaId))
            .ToListAsync(cancellationToken);

        return cities.ToDictionary(x => x.HhAreaId, x => x);
    }
}

