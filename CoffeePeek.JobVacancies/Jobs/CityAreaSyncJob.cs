using CoffeePeek.JobVacancies.Services;

namespace CoffeePeek.JobVacancies.Jobs;

public class CityAreaSyncJob(
    IHhApiService hhApiService,
    ICitySyncService citySyncService,
    ILogger<CityAreaSyncJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting CityAreaSyncJob...");

        try
        {
            var hhAreas = await hhApiService.GetAreas(cancellationToken);
            if (hhAreas.Count == 0)
            {
                logger.LogWarning("No areas found in hh.ru response");
                return;
            }

            var stats = await citySyncService.SyncCitiesAsync(hhAreas, cancellationToken);

            logger.LogInformation("CityAreaSyncJob finished: updated={Updated}, notFound={NotFound}", stats.updated, stats.notFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CityAreaSyncJob failed");
            throw;
        }
    }
}

