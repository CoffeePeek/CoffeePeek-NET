using System.Text.Json;
using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Infrastructure.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace OutboxBackgroundService;

/// <summary>
/// Generic outbox processor that can work with any DbContext containing OutboxEvent entities.
/// </summary>
public class OutboxProcessor<TOutboxEvent, TDbContext>(
    TDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ILogger<OutboxProcessor<TOutboxEvent, TDbContext>> logger)
    : IOutboxProcessor
    where TOutboxEvent : class, IOutboxEventEntity
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    private readonly IPublishEndpoint _publishEndpoint =
        publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));

    private readonly ILogger<OutboxProcessor<TOutboxEvent, TDbContext>> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task ProcessOutboxEventsAsync(CancellationToken cancellationToken)
    {
        var events = await _dbContext.Set<TOutboxEvent>()
            .Where(x => !x.Processed)
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (events.Count == 0)
            return;

        _logger.LogDebug("Processing {Count} outbox events from {DbContext}", events.Count, typeof(TDbContext).Name);

        foreach (var e in events)
        {
            try
            {
                var fullTypeName = $"CoffeePeek.Contract.Events.{e.EventType}";
                if (string.IsNullOrEmpty(e.EventType))
                {
                    _logger.LogWarning("Outbox event {Id} has empty EventType", e.Id);
                    continue;
                }

                var contractAssembly = typeof(UserRegisteredEvent).Assembly;
                var eventType = contractAssembly.GetTypes()
                    .FirstOrDefault(t => t.Name.Equals(e.EventType, StringComparison.OrdinalIgnoreCase));

                if (eventType == null)
                {
                    _logger.LogError("Could not find event type {EventType} in assembly {Assembly} for outbox event {Id}", e.EventType, contractAssembly.FullName, e.Id);
                    e.Processed = true;
                    e.ProcessedAt = DateTime.UtcNow;
                    continue;
                }

                var message = JsonSerializer.Deserialize(e.Payload, eventType, _jsonOptions);
                if (message == null)
                {
                    _logger.LogError("Could not deserialize payload for outbox event {Id}", e.Id);
                    e.Processed = true;
                    e.ProcessedAt = DateTime.UtcNow;
                    continue;
                }

                await _publishEndpoint.Publish(message, cancellationToken);

                e.Processed = true;
                e.ProcessedAt = DateTime.UtcNow;

                _logger.LogDebug("Successfully published outbox event {Id} of type {EventType}", e.Id, e.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox event {Id} from {DbContext}", e.Id,
                    typeof(TDbContext).Name);
                // Don't mark as processed on error - will retry on next iteration
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}