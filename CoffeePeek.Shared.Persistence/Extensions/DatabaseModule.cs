using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

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
            // jsonb collections (List<string> Signals/TagSlugs) require dynamic JSON on the
            // Npgsql data source. Without it, materializing ShopImportCandidates throws
            // InvalidCastException and YARP surfaces the aborted response as HTTP 502.
            opt.UseNpgsql(
                connectionString,
                npgsql => npgsql.ConfigureDataSource(ds => ds.EnableDynamicJson()));
            configure?.Invoke(opt);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();

        return services;
    }
}

