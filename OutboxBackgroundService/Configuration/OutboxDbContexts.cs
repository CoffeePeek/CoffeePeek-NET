using CoffeePeek.Auth.Infrastructure.Persistent;
using CoffeePeek.Moderation.Domain;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Infrastructure;
using CoffeePeek.Shops.Infrastructure.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace OutboxBackgroundService.Configuration;

public static class OutboxDbContexts
{
    public static void RegisterOutboxDbContexts(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Auth Service Outbox
        var authConnectionString = GetConnectionString(configuration, "AccountService");
        if (!string.IsNullOrEmpty(authConnectionString))
        {
            services.AddDbContext<AccountDbContext>(opt => 
                opt.UseNpgsql(authConnectionString));
            
            services.AddScoped<IOutboxProcessor>(provider =>
            {
                var dbContext = provider.GetRequiredService<AccountDbContext>();
                var publishEndpoint = provider.GetRequiredService<IPublishEndpoint>();
                var logger = provider.GetRequiredService<ILogger<OutboxProcessor<CoffeePeek.Account.Domain.Entities.OutboxEvent, AccountDbContext>>>();
                return new OutboxProcessor<CoffeePeek.Account.Domain.Entities.OutboxEvent, AccountDbContext>(dbContext, publishEndpoint, logger);
            });
        }

        // Shops Service Outbox
        var shopsConnectionString = GetConnectionString(configuration, "ShopsService");
        if (!string.IsNullOrEmpty(shopsConnectionString))
        {
            services.AddDbContext<ShopsDbContext>(opt => 
                opt.UseNpgsql(shopsConnectionString));
            
            services.AddScoped<IOutboxProcessor>(provider =>
            {
                var dbContext = provider.GetRequiredService<ShopsDbContext>();
                var publishEndpoint = provider.GetRequiredService<IPublishEndpoint>();
                var logger = provider.GetRequiredService<ILogger<OutboxProcessor<OutboxEvent, ShopsDbContext>>>();
                return new OutboxProcessor<OutboxEvent, ShopsDbContext>(dbContext, publishEndpoint, logger);
            });
        }

        // Moderation Service Outbox
        var moderationConnectionString = GetConnectionString(configuration, "ModerationService");
        if (!string.IsNullOrEmpty(moderationConnectionString))
        {
            services.AddDbContext<ModerationDbContext>(opt => 
                opt.UseNpgsql(moderationConnectionString));
            
            services.AddScoped<IOutboxProcessor>(provider =>
            {
                var dbContext = provider.GetRequiredService<ModerationDbContext>();
                var publishEndpoint = provider.GetRequiredService<IPublishEndpoint>();
                var logger = provider.GetRequiredService<ILogger<OutboxProcessor<CoffeePeek.Shops.Domain.Entities.OutboxEvent, ModerationDbContext>>>();
                return new OutboxProcessor<CoffeePeek.Shops.Domain.Entities.OutboxEvent, ModerationDbContext>(dbContext, publishEndpoint, logger);
            });
        }
    }

    private static string? GetConnectionString(IConfiguration configuration, string serviceName)
    {
        // Try service-specific configuration first
        var serviceConnectionString = configuration[$"{serviceName}__PostgresCpOptions__ConnectionString"];
        if (!string.IsNullOrEmpty(serviceConnectionString))
        {
            return serviceConnectionString;
        }

        // Try default PostgresCpOptions (for backward compatibility with AuthService)
        if (serviceName == "AuthService")
        {
            var defaultConnectionString = configuration["PostgresCpOptions:ConnectionString"];
            if (!string.IsNullOrEmpty(defaultConnectionString))
            {
                return defaultConnectionString;
            }
        }

        // Try environment variable
        var envKey = $"{serviceName}__PostgresCpOptions__ConnectionString";
        var envConnectionString = Environment.GetEnvironmentVariable(envKey);
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        return null;
    }
}

