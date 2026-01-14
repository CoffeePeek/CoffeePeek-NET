namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

public record ShopSchedule
{
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsClosed { get; private set; }
    public ICollection<ShopScheduleInterval> Intervals { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ShopSchedule()
    {
    }

    internal ShopSchedule(
        DayOfWeek dayOfWeek,
        bool isClosed,
        ICollection<ShopScheduleInterval> intervals)
    {
        DayOfWeek = dayOfWeek;
        IsClosed = isClosed;
        Intervals = intervals;
    }
}