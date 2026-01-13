namespace CoffeePeek.Shops.Domain.Entities;

public record ShopSchedule
{
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsClosed { get; private set; }
    public IReadOnlyCollection<ShopScheduleInterval> Intervals { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ShopSchedule()
    {
    }

    internal ShopSchedule(
        DayOfWeek dayOfWeek,
        bool isClosed,
        IReadOnlyCollection<ShopScheduleInterval> intervals)
    {
        DayOfWeek = dayOfWeek;
        IsClosed = isClosed;
        Intervals = intervals;
    }
}