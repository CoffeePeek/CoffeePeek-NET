using CoffeePeek.AuthService.Repositories;
using CoffeePeek.Contract.Events;
using MassTransit;

namespace CoffeePeek.AuthService.Utils;

public class OutboxPublisher(IServiceProvider serviceProvider, IPublishEndpoint publishEndpoint)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IUserCredentialsRepository>();

            // Получаем необработанные события
            var events = await repo.GetUnprocessedOutboxEvents(stoppingToken);

            foreach (var @event in events)
            {
                try
                {
                    var payload = System.Text.Json.JsonSerializer.Deserialize<UserRegisteredEvent>(@event.Payload)!;
                    await publishEndpoint.Publish(payload, stoppingToken);

                    // Отмечаем событие как обработанное
                    await repo.MarkOutboxEventAsProcessed(@event.Id, stoppingToken);
                }
                catch
                {
                    // Ошибка — оставляем событие для повторной отправки
                }
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}