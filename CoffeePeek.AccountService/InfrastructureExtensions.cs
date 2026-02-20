using CoffeePeek.Account.Application;
using CoffeePeek.Account.Application.Features.Auth.RegisterUser;
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
        builder.WebHost
            .ConfigureWebhost();
        
        var handlersAssembly = typeof(RegisterUserHandler).Assembly;
        builder.Host
            .AddPersistence(handlersAssembly);
        
        builder.Services
            .AddApplication()
            .AddInfrastructure()
            .AddPersistence(builder)
            .AddPresentation();
        
        return builder;
    }

    public static void UseApplication(this WebApplication app)
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