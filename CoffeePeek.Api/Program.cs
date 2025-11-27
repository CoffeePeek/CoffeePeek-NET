using CoffeePeek.Api.Middleware;
using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.BuildingBlocks.EfCore;
using CoffeePeek.BuildingBlocks.Extensions;
using CoffeePeek.BuildingBlocks.RedisOptions;
using CoffeePeek.BuildingBlocks.Sentry;
using CoffeePeek.BusinessLogic.Configuration;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.BuildSentry();

builder.Services.AddEndpointsApiExplorer();

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services.RedisConfigurationOptions();

builder.Services
    .ConfigureMapster()
    .AddSwagger()
    .AddBearerAuthentication()
    .AddValidators()
    .RegisterInfrastructure()
    .PostgresConfigure()
    .ConfigureBusinessLogic()
    .AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    
    if (!await roleManager.RoleExistsAsync(RoleConsts.Admin))
    {
        await roleManager.CreateAsync(new Role {Name = RoleConsts.Admin});
        await roleManager.CreateAsync(new Role {Name = RoleConsts.Merchant});
        await roleManager.CreateAsync(new Role {Name = RoleConsts.User});
    }
}


app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<UserTokenMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();

