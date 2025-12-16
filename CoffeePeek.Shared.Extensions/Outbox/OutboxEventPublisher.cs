using System.Text.Json;
using CoffeePeek.Shared.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;

namespace CoffeePeek.Shared.Extensions.Outbox;

/// <summary>
/// Generic implementation of outbox event publisher.
/// Works with any DbContext that has a DbSet of outbox events.
/// </summary>
/// <typeparam name="TOutboxEvent">Type of outbox event entity in the database</typeparam>
/// <typeparam name="TDbContext">Type of database context</typeparam>
public class OutboxEventPublisher<TOutboxEvent, TDbContext> : IOutboxEventPublisher
    where TOutboxEvent : class, IOutboxEventEntity, new()
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string AgentLogPath = @"c:\Users\User\RiderProjects\CoffeePeek-BackEnd\.cursor\debug.log";

    public OutboxEventPublisher(TDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    private static void WriteAgentLog(object payload)
    {
        try
        {
            var directory = Path.GetDirectoryName(AgentLogPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(AgentLogPath, JsonSerializer.Serialize(payload) + Environment.NewLine);
        }
        catch
        {
            // swallow logging errors to avoid impacting runtime flow
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventTypeName = typeof(TEvent).Name;
        var payload = JsonSerializer.Serialize(@event, _jsonOptions);

        // #region agent log
        WriteAgentLog(new
        {
            sessionId = "debug-session",
            runId = "pre-fix",
            hypothesisId = "H1",
            location = "OutboxEventPublisher.PublishAsync",
            message = "outbox publish called",
            data = new { eventTypeName, payloadLength = payload?.Length },
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
        // #endregion

        var outboxEvent = new TOutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventTypeName,
            Payload = payload,
            CreatedAt = DateTime.UtcNow,
            Processed = false
        };

        var dbSet = _dbContext.Set<TOutboxEvent>();
        await dbSet.AddAsync(outboxEvent, cancellationToken);

        // Note: SaveChanges should be called by the caller (handler) as part of the same transaction
        // This ensures the event is saved together with business data
    }
}

