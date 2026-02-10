using CoffeePeek.Account.Application;
using CoffeePeek.Account.Infrastructure;
using CoffeePeek.Account.Persistence;
using CoffeePeek.Shared.Web.Logging;
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
            app.MapOpenApi();
            //await app.ApplyMigrations<AccountDbContext>();
        }
        
        app.MapDefaultEndpoints();

        app.MapControllers();
    }
}