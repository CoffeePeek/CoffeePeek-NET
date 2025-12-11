using CoffeePeek.JobVacancies.Models;

namespace CoffeePeek.JobVacancies.Services;

public interface IJobVacancySyncService
{
    Task SyncVacanciesAsync(List<HhVacancyItem> vacancies, CancellationToken cancellationToken = default);
}