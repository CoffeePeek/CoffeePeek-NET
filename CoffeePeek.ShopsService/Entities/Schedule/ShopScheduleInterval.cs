namespace CoffeePeek.ShopsService.Entities;

public class ShopScheduleInterval
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }

    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }

    public virtual ShopSchedule Schedule { get; set; }
}