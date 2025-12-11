using CoffeePeek.JobVacancies.Repository;
using CoffeePeek.JobVacancies.Services;
using Hangfire;

namespace CoffeePeek.JobVacancies.Jobs;

public class HhVacanciesRecurringJob(
    IHhApiService client,
    IJobVacancySyncService syncService,
    ICityRepository cityRepository,
    ILogger<HhVacanciesRecurringJob> logger)
{
    public async Task ExecuteAsync(IJobCancellationToken cancellationToken)
    {
        logger.LogInformation("HhVacanciesRecurringJob started");

        var cities = await cityRepository.GetHhAreasAsync(cancellationToken.ShutdownToken);
        var searchTexts = new[] { "бариста", "обжарщик", "менеджер в кофейне" };

        var vacancies = await client.GetVacancies(searchTexts, cities.ToArray(), cancellationToken.ShutdownToken); 

        await syncService.SyncVacanciesAsync(vacancies, cancellationToken.ShutdownToken);
        
        logger.LogInformation("HhVacanciesRecurringJob finished: {count} vacancies synced", vacancies.Count);
    }
}