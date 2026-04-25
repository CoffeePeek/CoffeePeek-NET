namespace CoffeePeek.Shared.Persistence.Options;

public class RedisOptions
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string Password { get; set; } = null!;
}