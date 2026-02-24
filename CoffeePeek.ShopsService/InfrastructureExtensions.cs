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

        var handlersAssembly = typeof(GetCoffeeShopHandler).Assembly;
        builder.AddWolverine(handlersAssembly);
        
        builder.Services
            .AddApplication()
            .AddPersistence(builder)
            .AddInfrastructure()
            .AddPresentation();

        return builder;
    }

    public static void UseApplication(this WebApplication app)
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

