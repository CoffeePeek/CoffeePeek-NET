namespace CoffeePeek.Shared.Extensions.Configuration;

public static class DatabaseConnectionHelper
{
    public static string? GetDatabaseConnectionString()
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            try
            {
                var uri = new Uri(databaseUrl);
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;
                var database = uri.AbsolutePath.TrimStart('/');
                var user = uri.UserInfo.Split(':')[0];
                var password = uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : "";
                
                return $"Host={host};Port={port};Database={database};Username={user};Password={password};SslMode=Prefer;TrustServerCertificate=true;";
            }
            catch
            {
                //ignore
            }
        }
        
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        var pgPort = Environment.GetEnvironmentVariable("PGPORT");
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
        
        if (!string.IsNullOrEmpty(pgHost) && !string.IsNullOrEmpty(pgDatabase) && 
            !string.IsNullOrEmpty(pgUser) && !string.IsNullOrEmpty(pgPassword))
        {
            var port = !string.IsNullOrEmpty(pgPort) ? pgPort : "5432";
            return $"Host={pgHost};Port={port};Database={pgDatabase};Username={pgUser};Password={pgPassword};SslMode=Prefer;TrustServerCertificate=true;";
        }
        
        var directConnectionString = Environment.GetEnvironmentVariable("PostgresCpOptions__ConnectionString");
        if (!string.IsNullOrEmpty(directConnectionString))
        {
            return directConnectionString;
        }
        
        return null;
    }
}


