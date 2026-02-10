using System.Reflection;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.PhotoMetadataAggregate;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using CoffeePeek.Account.Persistence.Repositories;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Extentions;
using CoffeePeek.Shared.Persistence;
using CoffeePeek.Shared.Persistence.Data;
using CoffeePeek.Shared.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoffeePeek.Account.Persistence;

public static class DependencyInjection
{
    public static string GetConnectionString(IConfiguration configuration, IServiceCollection services)
    {
        if (configuration["DOTNET_ASPIRE"] == "true")
        {
            return configuration.GetConnectionString(AppResources.AccountDb) ?? 
                   throw new InvalidOperationException("Connection string not found");
        }
        else
        {
            return services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
        }
    }
    
    public static IHostBuilder AddPersistence(this IHostBuilder hostBuilder, Assembly handlersAssembly, string connectionString)
    {
        hostBuilder.AddWolverine(handlersAssembly, connectionString);
        
        return hostBuilder;
    }
    
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, WebApplicationBuilder builder)
    {
        // Database
        string connectionString = GetConnectionString(configuration, services);
        
        if (configuration["DOTNET_ASPIRE"] == "true")
        {
            builder.AddNpgsqlDbContext<AccountDbContext>(
                connectionName:AppResources.AccountDb, 
                configureDbContextOptions: opt => opt.AddInterceptors(new AuditInterceptor()),
                configureSettings: settings => { settings.DisableRetry = true; }
                );
        }
        else
        {
            services.AddDbContext<AccountDbContext>(opt => opt.UseNpgsql(connectionString).AddInterceptors(new AuditInterceptor()));
        }

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