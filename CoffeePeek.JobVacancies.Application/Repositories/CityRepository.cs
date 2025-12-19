using CoffeePeek.JobVacancies.Domain.Entities;
using CoffeePeek.JobVacancies.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.JobVacancies.Application.Repositories;

public class CityRepository(IGenericRepository<CityMap> citiesRepository, IUnitOfWork unitOfWork) : ICityRepository
{
    public async Task<IReadOnlyList<int>> GetHhAreasAsync(CancellationToken cancellationToken = default)
    {
        return await citiesRepository
            .QueryAsNoTracking()
            .Select(x => x.HhAreaId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }

    public async Task UpsertAsync(CityMap map, CancellationToken cancellationToken = default)
    {
        var existing = await citiesRepository.FirstOrDefaultAsync(x => x.CityId == map.CityId, cancellationToken);
        if (existing is null)
        {
            if (map.Id == Guid.Empty)
            {
                map.Id = Guid.NewGuid();
            }

            map.UpdatedAt = DateTime.UtcNow;
            citiesRepository.Add(map);
        }
        else
        {
            existing.HhAreaId = map.HhAreaId;
            existing.CityName = map.CityName;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CityMap?> GetByHhAreaIdAsync(int hhAreaId, CancellationToken cancellationToken = default)
    {
        return await citiesRepository.FirstOrDefaultAsNoTrackingAsync(x => x.HhAreaId == hhAreaId, cancellationToken);
    }

    public async Task<Dictionary<int, CityMap>> GetByHhAreaIdsAsync(IEnumerable<int> hhAreaIds,
        CancellationToken cancellationToken = default)
    {
        var areaIdsList = hhAreaIds.ToList();
        if (areaIdsList.Count == 0)
        {
            return new Dictionary<int, CityMap>();
        }

        var cities = await citiesRepository
            .QueryAsNoTracking()
            .Where(x => areaIdsList.Contains(x.HhAreaId))
            .GroupBy(x => x.HhAreaId)
            .ToDictionaryAsync(g => g.Key, g => g.First(), cancellationToken);

        return cities;
    }
}