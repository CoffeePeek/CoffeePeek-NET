namespace OutboxBackgroundService;

public class PostgresCpOptions
{
    public string AccountConnectionString { get; set; }
    public string ModerationConnectionString { get; set; }
    public string ShopsConnectionString { get; set; }
}