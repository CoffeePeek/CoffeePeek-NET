using CoffeePeek.Contract.Dtos.Schedule;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

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

    public static ShopSchedule Create(DayOfWeek dayOfWeek, bool isClosed, List<ShopScheduleIntervalDto> intervalDtos)
    {
        var intervals = intervalDtos.Select(x => ShopScheduleInterval.Create(x.OpenTime, x.CloseTime)).ToList();
        return new ShopSchedule(dayOfWeek, isClosed, intervals);
    }
}