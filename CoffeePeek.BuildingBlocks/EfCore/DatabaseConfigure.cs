using CoffeePeek.Domain.Databases;
using CoffeePeek.Shared.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.BuildingBlocks.EfCore;

public static class DatabaseConfigure
{
    public static IServiceCollection PostgresConfigure(this IServiceCollection services)
    {
        var dbOptions = services.AddValidateOptions<PostgresCpOptions>();
        services
            .AddDbContext<CoffeePeekDbContext>(opt =>
            {
                opt.UseNpgsql(dbOptions.ConnectionString, b => b.MigrationsAssembly("CoffeePeek.Domain"));
            })
            .ConfigureDbRepositories();
        
        return services;
    }
}