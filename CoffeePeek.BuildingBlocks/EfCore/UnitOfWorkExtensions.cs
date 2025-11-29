using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Address;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Repositories;
using CoffeePeek.Domain.Repositories.Interfaces;
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

    public static IServiceCollection AddSpecificRepository<TEntity, TInterface, TRepository>(
        this IServiceCollection services)
        where TEntity : class
        where TInterface : class, IRepository<TEntity>
        where TRepository : class, TInterface
    {
        services.AddScoped<TInterface, TRepository>();

        services.AddScoped<IRepository<TEntity>, TRepository>(sp =>
            sp.GetRequiredService<TInterface>() as TRepository ??
            throw new InvalidOperationException("Repository resolution failed."));

        services.AddScoped<TRepository>();

        return services;
    }
    
    public static void ConfigureDbRepositories(this IServiceCollection services)
    {
        services.AddUnitOfWork<CoffeePeekDbContext>();
        
        services.AddSpecificRepository<City, ICityRepository, CityRepository>();
        
        services.AddSpecificRepository<ModerationShop, IModerationShopsRepository, ModerationShopsRepository>();
    }
}