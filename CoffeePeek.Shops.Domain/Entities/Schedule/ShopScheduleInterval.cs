namespace CoffeePeek.Shops.Domain.Entities;

public class ShopScheduleInterval : Entity<Guid>
{
    public Guid ScheduleId { get; set; }

    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }

    public ShopSchedule Schedule { get; set; }
}