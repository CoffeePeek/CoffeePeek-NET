using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CoffeePeek.Shops.Persistance.Configuration;

public class ShopsDbContextFactory : IDesignTimeDbContextFactory<ShopsDbContext>
{
    public ShopsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<ShopsDbContext>();

        var connectionString = configuration.GetSection("PostgresCpOptions:ConnectionString").Value;

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("ConnectionString not found in environment variables.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new ShopsDbContext(optionsBuilder.Options);
    }
}