namespace CoffeePeek.Shared.Extensions.Configuration;

public static class DatabaseConnectionHelper
{
    public static string? GetDatabaseConnectionString()
    {
        var directConnectionString = Environment.GetEnvironmentVariable("PostgresCpOptions__ConnectionString");
        if (!string.IsNullOrEmpty(directConnectionString))
        {
            return directConnectionString;
        }
        
        return null;
    }
}

