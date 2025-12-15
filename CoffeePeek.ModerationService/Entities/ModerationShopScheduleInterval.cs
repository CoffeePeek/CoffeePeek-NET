namespace CoffeePeek.ModerationService.Entities;

public class ModerationShopScheduleInterval
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }

    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }

    public virtual ModerationShopSchedule Schedule { get; set; }
}