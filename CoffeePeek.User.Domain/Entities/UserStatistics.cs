namespace CoffeePeek.UserService.Models;

public class UserStatistics
{
    public Guid UserId { get; set; }
    public int CheckInCount { get; set; }
    public int ReviewCount { get; set; }
    public int AddedShopsCount { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual User.Domain.Entities.User? User { get; set; }
}