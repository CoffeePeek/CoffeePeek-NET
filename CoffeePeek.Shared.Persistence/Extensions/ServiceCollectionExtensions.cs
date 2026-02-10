using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEfCoreData<TDbContext>(string connectionString,
            Action<DbContextOptionsBuilder>? additionalOptions = null)
            where TDbContext : DbContext
        {
            services.AddDbContext<TDbContext>(options =>
            {
                options.UseNpgsql(connectionString).AddInterceptors(new AuditInterceptor());
                additionalOptions?.Invoke(options);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();

            return services;
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