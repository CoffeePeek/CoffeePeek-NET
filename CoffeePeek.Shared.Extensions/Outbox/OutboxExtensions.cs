using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Outbox;

public static class OutboxExtensions
{
    /// <summary>
    /// Registers outbox event publisher for a specific service.
    /// </summary>
    /// <typeparam name="TOutboxEvent">Type of outbox event entity</typeparam>
    /// <typeparam name="TDbContext">Type of database context</typeparam>
    public static IServiceCollection AddOutboxEventPublisher<TOutboxEvent, TDbContext>(
        this IServiceCollection services)
        where TOutboxEvent : class, IOutboxEventEntity, new()
        where TDbContext : DbContext
    {
        services.AddScoped<IOutboxEventPublisher>(
            provider =>
            {
                var dbContext = provider.GetRequiredService<TDbContext>();
                return new OutboxEventPublisher<TOutboxEvent, TDbContext>(dbContext);
            });

        return services;
    }
}

