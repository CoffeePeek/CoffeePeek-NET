using CoffeePeek.Api.Middleware;
using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.BuildingBlocks.EfCore;
using CoffeePeek.BuildingBlocks.Extensions;
using CoffeePeek.BuildingBlocks.Sentry;
using CoffeePeek.BusinessLogic.Configuration;
using CoffeePeek.Infrastructure.Configuration;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Swagger;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Настройка для прослушивания на IPv6 (требуется для Railway приватной сети)
// IPv6 слушает на всех интерфейсах, включая IPv4 через IPv6-mapped адреса
builder.WebHost.UseUrls("http://[::]:80");

builder.WebHost.BuildSentry();

builder.Services.AddEndpointsApiExplorer();

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services.RedisConfigurationOptions();

builder.Services
    .ConfigureMapster()
    .AddSwagger("Coffee Peek API", "v1")
    .AddBearerAuthentication()
    .AddValidators()
    .RegisterInfrastructure()
    .PostgresConfigure()
    .ConfigureBusinessLogic()
    .AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<UserTokenMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    await IdentitySeed(app);
    app.UseHttpsRedirection();
}

app.MapControllers();

Console.WriteLine("Listening on: http://[::]:80 (IPv6 for Railway private network)");

app.Run();

return;

//todo: TASK:CP-65 move to another place
async Task IdentitySeed(WebApplication webApplication)
{
    using var scope = webApplication.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    if (!await roleManager.RoleExistsAsync(RoleConsts.Admin))
    {
        string[] roles = [RoleConsts.Admin, RoleConsts.Merchant, RoleConsts.User];
        
        foreach (var role in roles)
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }
}

