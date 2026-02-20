using System.Reflection;
using CoffeePeek.Account.Domain.Entities.PhotoMetadataAggregate;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using CoffeePeek.Account.Persistence.Repositories;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Persistence;
using CoffeePeek.Shared.Persistence.Data;
using CoffeePeek.Shared.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoffeePeek.Account.Persistence;

public static class DependencyInjection
{
    public static IHostBuilder AddPersistence(this IHostBuilder hostBuilder, Assembly handlersAssembly)
    {
        hostBuilder.AddWolverine(handlersAssembly);

        return hostBuilder;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, WebApplicationBuilder builder)
    {
#if DEBUG
        builder.AddNpgsqlDbContext<AccountDbContext>(
            connectionName: AppResources.AccountDb,
            configureDbContextOptions: opt => opt.AddInterceptors(new AuditInterceptor()),
            configureSettings: settings => { settings.DisableRetry = true; }
        );
    
        services.AddScoped<IUnitOfWork, UnitOfWork<AccountDbContext>>();
#else
        var connectionString = GetConnectionString(services);

        services.AddDatabase<AccountDbContext>(
            connectionString,
            opt => opt.AddInterceptors(new AuditInterceptor())
        );
        
        static string GetConnectionString(IServiceCollection services)
        {
            return services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
        }
#endif
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        
        // 2. Repository Implementations
        services.AddScoped<UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPhotoMetadataRepository, PhotoMetadataRepository>();
        services.AddScoped<IQueryUserRepository, QueryUserRepository>();

        // 3. Repository Decorators (после базовых репозиториев)
        services.AddScoped<IUserRepository>(provider =>
        {
            var baseRepo = provider.GetRequiredService<UserRepository>();
            var redisService = provider.GetRequiredService<ICacheService>();
            return new CachedUserRepository(baseRepo, redisService);
        });

        services.AddCacheModule();

        return services;
    }
}