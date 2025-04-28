using CoffeePeek.Domain.Databases;
using CoffeePeek.Shared.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.BuildingBlocks.EfCore;

public static class DatabaseConfigure
{
    public static void Configure(this IServiceCollection services)
    {
        var dbOptions = services.AddValidateOptions<PostgresCpOptions>();
        services
            .AddDbContext<CoffeePeekDbContext>(opt =>
            {
                opt.UseNpgsql(dbOptions.ConnectionString, b => b.MigrationsAssembly("CoffeePeek.Data"));
            })
            .ConfigureDbRepositories();
    }
}