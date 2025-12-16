using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace CoffeePeek.Shared.Extensions.Modules;

public static class HealthCheckModule
{
    /// <summary>
    /// Adds health checks for database, RabbitMQ, and Redis.
    /// </summary>
    public static IServiceCollection AddHealthCheckModule(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "self" });

            return services;
        }

    /// <summary>
    /// Adds PostgreSQL health check.
    /// </summary>
    public static IServiceCollection AddPostgresHealthCheck(this IServiceCollection services,
        string connectionString,
        string name = "postgres")
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return services;
            }

            services.AddHealthChecks()
                .AddNpgSql(
                    connectionString,
                    name: name,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "db", "postgres", "ready" });

            return services;
        }

    /// <summary>
    /// Adds RabbitMQ health check.
    /// </summary>
    public static IServiceCollection AddRabbitMqHealthCheck(this IServiceCollection services,
        string hostName,
        ushort port,
        string username,
        string password,
        string name = "rabbitmq")
        {
            if (string.IsNullOrEmpty(hostName))
            {
                return services;
            }

            services.AddHealthChecks()
                .AddRabbitMQ(
                    sp =>
                    {
                        var factory = new ConnectionFactory
                        {
                            HostName = hostName,
                            Port = port,
                            UserName = username,
                            Password = password
                        };
                        return factory.CreateConnectionAsync();
                    },
                    name: name,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "messaging", "rabbitmq", "ready" });

            return services;
        }

    /// <summary>
    /// Adds Redis health check.
    /// </summary>
    public static IServiceCollection AddRedisHealthCheck(this IServiceCollection services,
        string host,
        int port,
        string? password = null,
        string name = "redis")
        {
            if (string.IsNullOrEmpty(host))
            {
                return services;
            }

            var connectionString = $"{host}:{port}";
            if (!string.IsNullOrEmpty(password))
            {
                connectionString = $"{host}:{port},password={password}";
            }

            services.AddHealthChecks()
                .AddRedis(
                    connectionString,
                    name: name,
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "cache", "redis", "ready" });

            return services;
        }

    /// <summary>
    /// Adds all health checks (PostgreSQL, RabbitMQ, Redis) based on provided options.
    /// </summary>
    public static IServiceCollection AddAllHealthChecks(this IServiceCollection services,
        PostgresCpOptions? dbOptions = null,
        RabbitMqOptions? rabbitMqOptions = null,
        RedisOptions? redisOptions = null)
        {
            services.AddHealthCheckModule();

            // PostgreSQL
            if (dbOptions != null && !string.IsNullOrEmpty(dbOptions.ConnectionString))
            {
                services.AddPostgresHealthCheck(dbOptions.ConnectionString);
            }

            // RabbitMQ
            if (rabbitMqOptions != null && !string.IsNullOrEmpty(rabbitMqOptions.HostName))
            {
                services.AddRabbitMqHealthCheck(
                    rabbitMqOptions.HostName,
                    rabbitMqOptions.Port,
                    rabbitMqOptions.Username,
                    rabbitMqOptions.Password);
            }

            // Redis
            if (redisOptions != null && !string.IsNullOrEmpty(redisOptions.Host))
            {
                services.AddRedisHealthCheck(redisOptions.Host, redisOptions.Port, redisOptions.Password);
            }

            return services;
        }
}
