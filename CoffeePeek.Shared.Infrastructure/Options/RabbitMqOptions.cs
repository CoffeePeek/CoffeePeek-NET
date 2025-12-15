namespace CoffeePeek.Shared.Infrastructure.Options;

public class RabbitMqOptions
{
    public string HostName { get; set; } = null!;
    public ushort Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}