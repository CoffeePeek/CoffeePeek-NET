using CoffeePeek.JobVacancies.Application.Models.HH.Responses;

namespace CoffeePeek.JobVacancies.Application.Services;

public interface ICitySyncService
{
    Task<(int updated, int notFound)> SyncCitiesAsync(List<HhAreaNode> hhAreas, CancellationToken cancellationToken = default);
}