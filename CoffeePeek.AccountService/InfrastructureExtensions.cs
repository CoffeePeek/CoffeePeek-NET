using CoffeePeek.Account.Application;
using CoffeePeek.Account.Infrastructure;
using CoffeePeek.Account.Persistence;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffePeek.ServiceDefaults;

namespace CoffeePeek.AccountService;

public static class InfrastructureExtensions
{
    public static WebApplicationBuilder AddApplication(this WebApplicationBuilder builder)
    {
        builder.AddSerilogLogging();
        builder.AddServiceDefaults();
        builder.WebHost.ConfigureWebhost();
        
        builder.Services
            .AddApplication()
            .AddInfrastructure()
            .AddPersistence()
            .AddPresentation();
        
        return builder;
    }

    public static WebApplication UseApplication(this WebApplication app)
    {
        app.UseAuthorization();
        
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            //AccountDbInitializer.SeedAsync(app.Services);
        }

        app.UseAuthorization();
        
        app.MapDefaultEndpoints();

        app.UseSwaggerDocumentation();

        app.MapControllers();
        return app;
    }
}