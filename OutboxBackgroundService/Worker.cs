using System.Text.Json;
using CoffeePeek.AuthService.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace OutboxBackgroundService;

public class Worker(ILogger<Worker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var events = await context.OutboxEvents
                .Where(x => !x.Processed)
                .OrderBy(x => x.CreatedAt)
                .Take(20)
                .ToListAsync(stoppingToken);

            foreach (var e in events)
            {
                try
                {
                    var fullTypeName = $"CoffeePeek.Contract.Events.{e.EventType}";

                    var contractAssembly = typeof(CoffeePeek.Contract.Events.UserRegisteredEvent).Assembly; 
                    var eventType = contractAssembly.GetType(fullTypeName, throwOnError: false, ignoreCase: true);
                    
                    var message = JsonSerializer.Deserialize(e.Payload, eventType!);

                    await publishEndpoint.Publish(message, stoppingToken);

                    e.Processed = true;
                    e.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing outbox event {Id}", e.Id);
                }
            }

            await context.SaveChangesAsync(stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}