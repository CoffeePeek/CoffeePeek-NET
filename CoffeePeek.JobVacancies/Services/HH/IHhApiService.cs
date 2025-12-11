using CoffeePeek.JobVacancies.Models;
using CoffeePeek.JobVacancies.Models.Responses;

namespace CoffeePeek.JobVacancies.Services;

public interface IHhApiService
{
    Task<List<HhAreaNode>> GetAreas(CancellationToken cancellationToken = default);

    Task<List<HhVacancyItem>> GetVacancies(IEnumerable<string> texts, int[] areaIds,
        CancellationToken cancellationToken = default);
}