using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.Repositories;
using CoffeePeek.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.BuildingBlocks.EfCore;

public static class UnitOfWorkExtensions
{
    public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddScoped<IRepositoryFactory, UnitOfWork<TContext>>();            
        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

        return services;
    }

    public static IServiceCollection AddCustomRepository<TEntity, TRepository>(this IServiceCollection services)
        where TEntity : class
        where TRepository : class, IRepository<TEntity>
    {
        services.AddScoped<IRepository<TEntity>, TRepository>();
        services.AddScoped<TRepository>();

        return services;
    }
    
    public static IServiceCollection ConfigureDbRepositories(this IServiceCollection services)
    {
        services.AddUnitOfWork<CoffeePeekDbContext>();

        services.AddCustomRepository<User, UserRepository>();
        
        services.AddCustomRepository<City, CityRepository>();
        
        services.AddCustomRepository<ModerationShop, ModerationShopsRepository>();

        services.AddCustomRepository<RefreshToken, RefreshTokenRepository>();
        
        return services;
    }
}