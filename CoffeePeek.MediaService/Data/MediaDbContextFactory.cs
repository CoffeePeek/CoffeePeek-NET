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

        // Design-time fallback for `dotnet ef` when PostgresCpOptions is not configured (runtime uses DI).
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString =
                "Host=127.0.0.1;Port=5432;Database=ef_design_time;Username=postgres;Password=postgres";
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new MediaDbContext(optionsBuilder.Options);
    }
}