using CoffeePeek.Account.Application;
using CoffeePeek.Account.Application.Features.Auth.RegisterUser;
using CoffeePeek.Account.Infrastructure;
using CoffeePeek.Account.Infrastructure.Consumers;
using CoffeePeek.Account.Persistence;
using CoffeePeek.Account.Persistence.Configuration;
using CoffeePeek.Shared.Persistence.Extensions;
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
        
        var applicationAssembly = typeof(RegisterUserHandler).Assembly;
        var infrastructureAssembly = typeof(CheckinCreatedHandler).Assembly;
        builder.AddWolverine([applicationAssembly, infrastructureAssembly]);
        
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
        }
        
        app.MapDefaultEndpoints();

        app.MapControllers();
    }
}