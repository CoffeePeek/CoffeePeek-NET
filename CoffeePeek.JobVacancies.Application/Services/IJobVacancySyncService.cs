using CoffeePeek.JobVacancies.Models;

namespace CoffeePeek.JobVacancies.Application.Services;

public interface IJobVacancySyncService
{
    Task SyncVacanciesAsync(List<HhVacancyItem> vacancies, CancellationToken cancellationToken = default);
}