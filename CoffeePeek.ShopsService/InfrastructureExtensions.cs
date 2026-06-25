using CoffeePeek.Shops.Application;
using CoffeePeek.Shops.Infrastructure;
using CoffeePeek.Shops.Persistance;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shared.Web.Logging;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Infrastructure.Consumers;
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

        var applicationAssembly = typeof(GetCoffeeShopHandler).Assembly;
        var infrastructureAssembly = typeof(ModerationShopApproveHandler).Assembly;
        builder.AddWolverine([applicationAssembly, infrastructureAssembly]);
        
        builder.Services
            .AddApplication()
            .AddPersistence(builder)
            .AddInfrastructure()
            .AddPresentation(builder.Configuration);

        return builder;
    }

    public static void UseApplication(this WebApplication app)
    {
        app.UseCoffeePeekRequestLogging();

        app.UseExceptionHandler();
        app.UseResponseCaching();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapDefaultEndpoints();

        app.MapControllers();
    }
}

