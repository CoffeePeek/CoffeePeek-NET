namespace CoffeePeek.Shared.Web;

public class RabbitMqOptions
{
    public string HostName { get; set; } = null!;
    public ushort Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string VirtualHost { get; set; } = null!;
}