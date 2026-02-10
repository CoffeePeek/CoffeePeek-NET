using CoffeePeek.Shops.Application;
using CoffeePeek.Shops.Infrastructure;
using CoffeePeek.Shops.Infrastructure.Configuration;
using CoffeePeek.Shops.Persistance;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffeePeek.Shared.Persistence.Extensions;
using CoffeePeek.Shared.Web.Logging;
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
            await app.ApplyMigrations<ShopsDbContext>();
            //await CoffeePeek.Shops.Infrastructure.ShopsDbInitializer.SeedAsync(app.Services);
        }

        app.MapDefaultEndpoints();

        // Swagger documentation
        app.UseSwaggerDocumentation();

        app.UseHttpsRedirection();

        app.MapControllers();
    }
}

