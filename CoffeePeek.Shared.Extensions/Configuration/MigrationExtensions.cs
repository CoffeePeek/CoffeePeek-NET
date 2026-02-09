using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePeek.Shared.Extensions.Configuration;

public static class MigrationExtensions
{
    public static async Task ApplyMigrations<TDbContext>(this IApplicationBuilder app) 
        where TDbContext : DbContext
    {
        try 
        {
            await using var scope = app.ApplicationServices.CreateAsyncScope();
            var serviceProvider = scope.ServiceProvider;
            
            // Try to get IDbContextFactory first (for DbContextPool from Aspire)
            var dbContextFactory = serviceProvider.GetService<IDbContextFactory<TDbContext>>();
            
            if (dbContextFactory != null)
            {
                // Use factory for DbContextPool
                await using var dbContext = await dbContextFactory.CreateDbContextAsync();
                await ApplyMigrationsToContext(dbContext);
            }
            else
            {
                // Fallback to direct resolution (for regular AddDbContext)
                var dbContext = serviceProvider.GetRequiredService<TDbContext>();
                await ApplyMigrationsToContext(dbContext);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying migrations: {ex.Message}");
            throw; // Re-throw to see the actual error
        }
    }
    
    private static async Task ApplyMigrationsToContext<TDbContext>(TDbContext dbContext) 
        where TDbContext : DbContext
    {
        await dbContext.Database.MigrateAsync();
    }
}