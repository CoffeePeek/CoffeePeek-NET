namespace OutboxBackgroundService.Models;

public class OutboxEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}