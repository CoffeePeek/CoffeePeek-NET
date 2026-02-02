using CoffeePeek.MediaService.Services;

namespace CoffeePeek.MediaService.BackgroundJobs;

public class PhotoCleanupBackgroundJob(
    IServiceProvider serviceProvider,
    ILogger<PhotoCleanupBackgroundJob> logger)
    : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Photo cleanup background job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<PhotoCleanupService>();

                await cleanupService.ProcessPendingDeletionsAsync(stoppingToken);

                logger.LogDebug("Photo cleanup cycle completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during photo cleanup");
            }

            await Task.Delay(CleanupInterval, stoppingToken);
        }

        logger.LogInformation("Photo cleanup background job stopped");
    }
}
