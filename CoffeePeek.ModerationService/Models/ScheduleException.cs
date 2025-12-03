namespace CoffeePeek.ModerationService.Models;

public class ScheduleException
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int ShopId { get; set; }
    public DateTime ExceptionDate { get; set; }
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
}

