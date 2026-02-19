using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Persistence.Extensions;

public static class DatabaseModule
{
    public static IServiceCollection AddDatabase<TDbContext>(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? configure = null)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>(opt =>
        {
            opt.UseNpgsql(connectionString);
            configure?.Invoke(opt);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();

        return services;
    }
}

