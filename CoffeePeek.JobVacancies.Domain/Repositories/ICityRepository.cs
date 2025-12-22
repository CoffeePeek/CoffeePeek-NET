using CoffeePeek.JobVacancies.Domain.Entities;

namespace CoffeePeek.JobVacancies.Domain.Repositories;

public interface ICityRepository
{
    Task<IReadOnlyList<int>> GetHhAreasAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(CityMap cityMap, CancellationToken ct);
    Task<CityMap?> GetByHhAreaIdAsync(int hhAreaId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, CityMap>> GetByHhAreaIdsAsync(IEnumerable<int> hhAreaIds, CancellationToken cancellationToken = default);
}