using CoffeePeek.Data.Interfaces;
using CoffeePeek.Data.Repositories;
using CoffeePeek.Shared.Extensions.Options;
using CoffeePeek.Shared.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Data.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет DbContext и базовые сервисы для работы с EF Core
    /// </summary>
    /// <typeparam name="TDbContext">Тип DbContext</typeparam>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="connectionString">Строка подключения к базе данных</param>
    /// <param name="additionalOptions">Дополнительные настройки для DbContext</param>
    /// <returns>Коллекция сервисов</returns>
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

    /// <summary>
    /// Добавляет DbContext и базовые сервисы для работы с EF Core используя PostgresCpOptions
    /// </summary>
    /// <typeparam name="TDbContext">Тип DbContext</typeparam>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="dbOptions">Опции подключения к базе данных</param>
    /// <param name="additionalOptions">Дополнительные настройки для DbContext</param>
    /// <returns>Коллекция сервисов</returns>
    public static IServiceCollection AddEfCoreData<TDbContext>(
        this IServiceCollection services,
        PostgresCpOptions dbOptions,
        Action<DbContextOptionsBuilder>? additionalOptions = null)
        where TDbContext : DbContext
    {
        return services.AddEfCoreData<TDbContext>(dbOptions.ConnectionString, additionalOptions);
    }

    /// <summary>
    /// Добавляет GenericRepository для конкретной сущности
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности</typeparam>
    /// <typeparam name="TDbContext">Тип DbContext</typeparam>
    /// <param name="services">Коллекция сервисов</param>
    /// <returns>Коллекция сервисов</returns>
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

