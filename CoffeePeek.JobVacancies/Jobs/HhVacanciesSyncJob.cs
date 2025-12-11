using CoffeePeek.JobVacancies.Models;
using CoffeePeek.JobVacancies.Repository;
using CoffeePeek.JobVacancies.Services;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;

namespace CoffeePeek.JobVacancies.Jobs;

public class HhVacanciesRecurringJob(
    IHhApiService client,
    IJobVacancySyncService syncService,
    ICityRepository cityRepository,
    IRedisService redisService,
    ILogger<HhVacanciesRecurringJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("HhVacanciesRecurringJob started");

        const string key = "hhvacncies";
        var cacheKey = CacheKey.Vacancies.HHVacancies(key);

        var cities = await cityRepository.GetHhAreasAsync(cancellationToken);
        var searchTexts = new[] { "бариста", "обжарщик", "менеджер в кофейне" };

        var cachedVacancies = await redisService.GetAsync<List<HhVacancyItem>>(cacheKey);

        var vacancies = cachedVacancies 
                        ?? await client.GetVacancies(searchTexts, cities.ToArray(), cancellationToken);

        await syncService.SyncVacanciesAsync(vacancies, cancellationToken);
        
        await redisService.SetAsync(cacheKey, vacancies, TimeSpan.FromHours(1));

        logger.LogInformation("HhVacanciesRecurringJob finished: {count} vacancies synced", vacancies.Count);
    }
}