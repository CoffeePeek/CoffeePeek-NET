using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoffeePeek.MediaService.Data;

public class MediaDbContextFactory : IDesignTimeDbContextFactory<MediaDbContext>
{
    public MediaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<MediaDbContext>();

        var connectionString = configuration.GetSection("PostgresCpOptions:ConnectionString").Value;

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("ConnectionString not found in environment variables.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new MediaDbContext(optionsBuilder.Options);
    }
}