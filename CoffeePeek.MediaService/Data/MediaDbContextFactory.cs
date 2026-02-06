using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoffeePeek.MediaService.Data;

public class MediaDbContextFactory : IDesignTimeDbContextFactory<MediaDbContext>
{
    public MediaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MediaDbContext>();
        
        var connectionString = Environment.GetEnvironmentVariable("PostgresCpOptions__ConnectionString");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("ConnectionString not found in environment variables.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new MediaDbContext(optionsBuilder.Options);
    }
}