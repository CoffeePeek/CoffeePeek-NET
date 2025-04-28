using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoffeePeek.BuildingBlocks.AuthOptions;

public class RoleInitializer(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        //var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRoleEntity>>();
    }   



    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}