using CoffeePeek.JobVacancies.Application.Models.HH;

namespace CoffeePeek.JobVacancies.Application.Services;

public interface IJobVacancySyncService
{
    Task SyncVacanciesAsync(List<HhVacancyItem> vacancies, CancellationToken cancellationToken = default);
}