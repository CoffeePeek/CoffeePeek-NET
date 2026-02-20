using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CoffeeShop.Moderation.Persistence.Configuration;

public class ModerationDbContextFactory : IDesignTimeDbContextFactory<ModerationDbContext>
{
    public ModerationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<ModerationDbContext>();

        var connectionString = configuration.GetSection("PostgresCpOptions:ConnectionString").Value;

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("ConnectionString not found in environment variables.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new ModerationDbContext(optionsBuilder.Options);
    }
}