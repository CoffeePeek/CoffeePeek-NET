using CoffeePeek.Shops.Application;
using CoffeePeek.Shops.Infrastructure;
using CoffeePeek.Shops.Persistance;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shared.Web.Logging;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Persistance.Configuration;
using CoffePeek.ServiceDefaults;
using Serilog;

namespace CoffeePeek.ShopsService;

public static class InfrastructureExtensions
{
    public static WebApplicationBuilder AddApplication(this WebApplicationBuilder builder)
    {
        builder.AddSerilogLogging();
        builder.AddServiceDefaults();
        builder.WebHost
            .ConfigureWebhost();

        var connectionString =
            Shops.Persistance.DependencyInjection.GetConnectionString(builder.Configuration, builder.Services);
        
        var handlersAssembly = typeof(GetCoffeeShopHandler).Assembly;
        builder.Host
            .AddPersistence(handlersAssembly, connectionString);
        
        builder.Services
            .AddApplication()
            .AddPersistence(builder.Configuration, builder)
            .AddInfrastructure()
            .AddPresentation();

        return builder;
    }

    public static async Task UseApplication(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            //await app.ApplyMigrations<ShopsDbContext>();
            //await CoffeePeek.Shops.Infrastructure.ShopsDbInitializer.SeedAsync(app.Services);
        }

        app.MapDefaultEndpoints();

        app.MapControllers();
    }
}

