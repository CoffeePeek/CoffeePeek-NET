using CoffeePeek.Account.Application;
using CoffeePeek.Account.Infrastructure;
using CoffeePeek.Account.Persistence;
using CoffeePeek.Account.Persistence.Configuration;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Swagger;
using CoffePeek.ServiceDefaults;
using Serilog;

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
            .AddPersistence(builder.Configuration, builder)
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
             await app.ApplyMigrations<AccountDbContext>();
            //AccountDbInitializer.SeedAsync(app.Services);
        }
        
        app.MapDefaultEndpoints();

        app.UseSwaggerDocumentation();

        app.MapControllers();
    }
}