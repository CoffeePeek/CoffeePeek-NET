using CoffeePeek.Api.Middleware;
using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.BuildingBlocks.EfCore;
using CoffeePeek.BuildingBlocks.Extensions;
using CoffeePeek.BuildingBlocks.RedisOptions;
using CoffeePeek.BuildingBlocks.Sentry;
using CoffeePeek.BusinessLogic.Configuration;
using CoffeePeek.Infrastructure.Configuration;

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

/*using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRoleEntity>>();
    
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRoleEntity("Admin"));
        await roleManager.CreateAsync(new IdentityRoleEntity("Merchant"));
        await roleManager.CreateAsync(new IdentityRoleEntity("User"));
    }
}*/


app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<UserTokenMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

