namespace CoffeePeek.ShopsService.Entities;

public class ShopSchedule
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsClosed { get; set; }

    public virtual Shop Shop { get; set; }
    public virtual ICollection<ShopScheduleInterval> Intervals { get; set; }
}