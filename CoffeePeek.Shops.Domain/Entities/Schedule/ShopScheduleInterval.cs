namespace CoffeePeek.Shops.Domain.Entities;

public class ShopScheduleInterval : Entity<Guid>
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }

    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }

    public virtual ShopSchedule Schedule { get; set; }
}