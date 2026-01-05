using System.Text.Json;
using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Infrastructure.Models;
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
        // Проверяем, сколько всего записей в таблице (включая обработанные)
        var totalCount = await _dbContext.Set<TOutboxEvent>().CountAsync(cancellationToken);
        _logger.LogInformation("Total outbox events in database: {Count}", totalCount);
    
        // Проверяем необработанные события
        var unprocessedCount = await _dbContext.Set<TOutboxEvent>()
            .Where(x => !x.Processed)
            .CountAsync(cancellationToken);
        _logger.LogInformation("Unprocessed outbox events: {Count}", unprocessedCount);
    
        // Пробуем получить все события для диагностики
        var allEvents = await _dbContext.Set<TOutboxEvent>()
            .Take(5)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Sample events (first 5): {Count}. Types: {Types}", 
            allEvents.Count,
            string.Join(", ", allEvents.Select(e => e.GetType().FullName)));
    
        var events = await _dbContext
            .Set<TOutboxEvent>()
            .Where(x => !x.Processed)
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (events.Count == 0)
        {
            _logger.LogWarning("No unprocessed events found. Total: {Total}, Unprocessed: {Unprocessed}", 
                totalCount, unprocessedCount);
            return;
        }

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