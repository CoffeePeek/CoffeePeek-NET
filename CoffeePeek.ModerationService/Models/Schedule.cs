namespace CoffeePeek.ModerationService.Models;

public class Schedule
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid ShopId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
}


