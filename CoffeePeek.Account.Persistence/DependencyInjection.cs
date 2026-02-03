using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using CoffeePeek.Account.Persistence.Repositories;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Modules;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Account.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // 1. Database & Repositories
        var dbOptions = services.AddValidateOptions<PostgresCpOptions>();
        services.AddEfCoreData<AccountDbContext>(dbOptions);
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
        services.AddCapModule<AccountDbContext>(dbOptions, "account-service");
        
        return services;
    }
}