namespace CoffeePeek.Shared.Extensions.Configuration;

public static class AppResources
{
    public const string AuthDb = "cp-auth-db";
    public const string JobVacanciesDb = "cp-jobvacancies-db";
    public const string ShopsDb = "cp-shops-db";
    public const string ModerationDb = "cp-moderation-db";
    public const string UserDb = "cp-user-db";

    public const string RedisCache = "cp-redis-cache";

    public const string RabbitMq = "cp-rabbitmq";
    
    public const string Postgres = "postgres";

    public const string AuthService = "coffeepeek-authservice";
    public const string UserService = "coffeepeek-userservice";
    public const string JobVacanciesService = "coffeepeek-jobvacancies";
    public const string ShopsService = "coffeepeek-shopsservice";
    public const string ModerationService = "coffeepeek-moderationservice";
    public const string OutboxBackgroundService = "outboxbackgroundservice";
    public const string Gateway = "coffeepeek-gateway";
}