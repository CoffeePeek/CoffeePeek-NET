using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Shops.Domain.Entities;

public class ShopSchedule : Entity<Guid>
{
    public Guid ShopId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsClosed { get; set; }

    public virtual Shop Shop { get; set; }
    public virtual ICollection<ShopScheduleInterval> Intervals { get; set; }
}