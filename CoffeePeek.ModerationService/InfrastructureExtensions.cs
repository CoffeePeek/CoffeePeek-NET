using CoffeePeek.Moderation.Application;
using CoffeePeek.Moderation.Application.Features.Shop.CreateShop;
using CoffeePeek.Moderation.Infrastructure;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shared.Web.Logging;
using CoffeeShop.Moderation.Persistence;
using CoffeeShop.Moderation.Persistence.Configuration;
using CoffePeek.ServiceDefaults;
using Serilog;

namespace CoffeePeek.ModerationService;

public static class InfrastructureExtensions
{
    public static WebApplicationBuilder AddApplication(this WebApplicationBuilder builder)
    {
        builder.AddSerilogLogging();
        builder.AddServiceDefaults();
        builder.WebHost
            .ConfigureWebhost();

        var connectionString =
            CoffeeShop.Moderation.Persistence.DependencyInjection.GetConnectionString(builder.Configuration, builder.Services);
        
        var handlersAssembly = typeof(SendCoffeeShopToModerationHandler).Assembly;
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

        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            await app.ApplyMigrations<ModerationDbContext>();
        }

        app.MapDefaultEndpoints();

        app.UseHttpsRedirection();

        app.MapControllers();
    }
}
