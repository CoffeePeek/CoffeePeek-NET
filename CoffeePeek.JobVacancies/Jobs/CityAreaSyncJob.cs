using CoffeePeek.JobVacancies.Services;
using Hangfire;

namespace CoffeePeek.JobVacancies.Jobs;

public class CityAreaSyncJob(
    IHhApiService hhApiService,
    ICitySyncService citySyncService,
    ILogger<CityAreaSyncJob> logger)
{
    public async Task ExecuteAsync(IJobCancellationToken cancellationToken)
    {
        logger.LogInformation("Starting CityAreaSyncJob...");

        try
        {
            var hhAreas = await hhApiService.GetAreas(cancellationToken.ShutdownToken);
            if (hhAreas.Count == 0)
            {
                logger.LogWarning("No areas found in hh.ru response");
                return;
            }

            var stats = await citySyncService.SyncCitiesAsync(hhAreas, cancellationToken.ShutdownToken);

            logger.LogInformation("CityAreaSyncJob finished: updated={Updated}, notFound={NotFound}", stats.updated, stats.notFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CityAreaSyncJob failed");
            throw;
        }
    }
}

