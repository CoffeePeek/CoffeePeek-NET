namespace OutboxBackgroundService;

public class Worker(ILogger<Worker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var processors = scope.ServiceProvider.GetServices<IOutboxProcessor>();

                var tasks = processors.Select(processor => 
                    processor.ProcessOutboxEventsAsync(stoppingToken).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            logger.LogError(task.Exception, "Error processing outbox events");
                        }
                    }, stoppingToken));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in outbox processing cycle");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}