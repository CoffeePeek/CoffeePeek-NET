using CoffeePeek.JobVacancies.Models.Responses;

namespace CoffeePeek.JobVacancies.Services;

public interface ICitySyncService
{
    Task<(int updated, int notFound)> SyncCitiesAsync(List<HhAreaNode> hhAreas, CancellationToken cancellationToken = default);
}