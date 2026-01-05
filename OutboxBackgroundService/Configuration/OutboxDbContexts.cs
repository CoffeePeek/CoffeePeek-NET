using CoffeePeek.Auth.Infrastructure.Persistent;
using CoffeePeek.Moderation.Infrastructure;
using CoffeePeek.Shops.Infrastructure.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace OutboxBackgroundService.Configuration;

public static class OutboxDbContexts
{
    public static void RegisterOutboxDbContexts(
        this IServiceCollection services,
        PostgresCpOptions options)
    {
        // Auth Service Outbox
        var authConnectionString = options.AccountConnectionString;
        if (!string.IsNullOrEmpty(authConnectionString))
        {
            services.AddDbContext<AccountDbContext>(opt =>
                opt.UseNpgsql(authConnectionString));

            services.AddScoped<IOutboxProcessor>(provider =>
            {
                var dbContext = provider.GetRequiredService<AccountDbContext>();
                var publishEndpoint = provider.GetRequiredService<IPublishEndpoint>();
                var logger = provider
                    .GetRequiredService<
                        ILogger<OutboxProcessor<CoffeePeek.Account.Domain.Events.OutboxEvent, AccountDbContext>>>();
                return new OutboxProcessor<CoffeePeek.Account.Domain.Events.OutboxEvent, AccountDbContext>(dbContext,
                    publishEndpoint, logger);
            });
        }

        // Shops Service Outbox
        var shopsConnectionString = options.ShopsConnectionString;
        if (!string.IsNullOrEmpty(shopsConnectionString))
        {
            services.AddDbContext<ShopsDbContext>(opt =>
                opt.UseNpgsql(shopsConnectionString));

            services.AddScoped<IOutboxProcessor>(provider =>
            {
                var dbContext = provider.GetRequiredService<ShopsDbContext>();
                var publishEndpoint = provider.GetRequiredService<IPublishEndpoint>();
                var logger = provider
                    .GetRequiredService<
                        ILogger<OutboxProcessor<CoffeePeek.Shops.Domain.Entities.OutboxEvent, ShopsDbContext>>>();
                return new OutboxProcessor<CoffeePeek.Shops.Domain.Entities.OutboxEvent, ShopsDbContext>(dbContext,
                    publishEndpoint, logger);
            });
        }

        // Moderation Service Outbox
        var moderationConnectionString = options.ModerationConnectionString;
        if (!string.IsNullOrEmpty(moderationConnectionString))
        {
            services.AddDbContext<ModerationDbContext>(opt =>
                opt.UseNpgsql(moderationConnectionString));

            services.AddScoped<IOutboxProcessor>(provider =>
            {
                var dbContext = provider.GetRequiredService<ModerationDbContext>();
                var publishEndpoint = provider.GetRequiredService<IPublishEndpoint>();
                var logger = provider
                    .GetRequiredService<ILogger<OutboxProcessor<CoffeePeek.Moderation.Domain.Entities.OutboxEvent,
                        ModerationDbContext>>>();
                return new OutboxProcessor<CoffeePeek.Moderation.Domain.Entities.OutboxEvent, ModerationDbContext>(
                    dbContext, publishEndpoint, logger);
            });
        }
    }
}