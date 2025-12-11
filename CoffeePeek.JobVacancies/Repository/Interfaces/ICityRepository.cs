using CoffeePeek.JobVacancies.Entities;

namespace CoffeePeek.JobVacancies.Repository;

public interface ICityRepository
{
    public Task<IReadOnlyList<int>> GetHhAreasAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(CityMap cityMap, CancellationToken ct);
    Task<CityMap?> GetByHhAreaIdAsync(int hhAreaId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, CityMap>> GetByHhAreaIdsAsync(IEnumerable<int> hhAreaIds, CancellationToken cancellationToken = default);
}

