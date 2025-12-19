namespace CoffeePeek.Moderation.Domain.Entities;

public class ModerationShopSchedule
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsClosed { get; set; }

    public virtual ModerationShop ModerationShop { get; set; }
    public virtual ICollection<ModerationShopScheduleInterval> Intervals { get; set; }
}
