using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Models;
using CoffeePeek.Shared.Infrastructure.Options;
using CoffeePeek.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Configuration;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEfCoreData<TDbContext, TOutboxEvent>(string connectionString,
            Action<DbContextOptionsBuilder>? additionalOptions = null)
            where TDbContext : DbContext
            where TOutboxEvent : OutboxEvent, new()
        {
            services.AddDbContext<TDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                additionalOptions?.Invoke(options);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext, TOutboxEvent>>();

            return services;
        }

        public IServiceCollection AddEfCoreData<TDbContext, TOutboxEvent>(PostgresCpOptions dbOptions,
            Action<DbContextOptionsBuilder>? additionalOptions = null)
            where TDbContext : DbContext
            where TOutboxEvent : OutboxEvent, new()
        {
            return services.AddEfCoreData<TDbContext, TOutboxEvent>(dbOptions.ConnectionString, additionalOptions);
        }

        public IServiceCollection AddGenericRepository<TEntity, TDbContext>()
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
}