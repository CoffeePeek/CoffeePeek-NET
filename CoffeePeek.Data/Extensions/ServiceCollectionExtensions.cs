using CoffeePeek.Data.Interfaces;
using CoffeePeek.Data.Repositories;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfCoreData<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? additionalOptions = null)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            additionalOptions?.Invoke(options);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();

        return services;
    }
    
    public static IServiceCollection AddEfCoreData<TDbContext>(
        this IServiceCollection services,
        PostgresCpOptions dbOptions,
        Action<DbContextOptionsBuilder>? additionalOptions = null)
        where TDbContext : DbContext
    {
        return services.AddEfCoreData<TDbContext>(dbOptions.ConnectionString, additionalOptions);
    }

    public static IServiceCollection AddGenericRepository<TEntity, TDbContext>(
        this IServiceCollection services)
        where TEntity : class
        where TDbContext : DbContext
    {
        services.AddScoped<IGenericRepository<TEntity>>(provider =>
        {
            var context = provider.GetRequiredService<TDbContext>();
            return new GenericRepository<TEntity, TDbContext>(context);
        });

        return services;
    }
}

