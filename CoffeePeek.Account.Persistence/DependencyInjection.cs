using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using CoffeePeek.Account.Persistence.Repositories;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Options;
using CoffeePeek.Shared.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoffeePeek.Account.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, WebApplicationBuilder builder)
    {
        // Database
        string connectionString;
        if (configuration["DOTNET_ASPIRE"] == "true")
        {
            connectionString = configuration.GetConnectionString(AppResources.AccountDb) ?? 
                               throw new InvalidOperationException("Connection string not found");
            builder.AddNpgsqlDbContext<AccountDbContext>(AppResources.AccountDb, configureDbContextOptions: opt => 
                opt.AddInterceptors(new AuditInterceptor()));
        }
        else
        {
            connectionString = services.AddValidateOptions<PostgresCpOptions>().ConnectionString;
            services.AddDbContext<AccountDbContext>(opt => opt.UseNpgsql(connectionString).AddInterceptors(new AuditInterceptor()));
        }

        // 1. Database & Repositories
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork<AccountDbContext>>();
        services.AddGenericRepository<UserCredential, AccountDbContext>();
        services.AddGenericRepository<Role, AccountDbContext>();
        services.AddGenericRepository<UserStatistics, AccountDbContext>();
        services.AddGenericRepository<User, AccountDbContext>();
        services.AddGenericRepository<PhotoMetadata, AccountDbContext>();

        // 2. Repository Implementations
        services.AddScoped<UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IQueryUserRepository, QueryUserRepository>();

        // 3. Repository Decorators (после базовых репозиториев)
        services.AddScoped<IUserRepository>(provider =>
        {
            var baseRepo = provider.GetRequiredService<UserRepository>();
            var redisService = provider.GetRequiredService<IRedisService>();
            return new CachedUserRepository(baseRepo, redisService);
        });
        
        // 4. Event Bus (CAP)
        services.AddCapModule(connectionString, AppResources.AccountService);
        
        return services;
    }
}