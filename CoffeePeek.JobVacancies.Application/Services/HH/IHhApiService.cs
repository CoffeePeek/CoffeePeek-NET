using CoffeePeek.JobVacancies.Application.Models.HH;
using CoffeePeek.JobVacancies.Application.Models.HH.Responses;

namespace CoffeePeek.JobVacancies.Application.Services;

public interface IHhApiService
{
    Task<List<HhAreaNode>> GetAreas(CancellationToken cancellationToken = default);

    Task<List<HhVacancyItem>> GetVacancies(IEnumerable<string> texts, int[] areaIds,
        CancellationToken cancellationToken = default);
}