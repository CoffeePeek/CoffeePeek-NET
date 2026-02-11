using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Moderation.Domain.Aggregates;

public record ModerationShopSchedule
{
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsClosed { get; private set; }
    public IReadOnlyCollection<ModerationShopScheduleInterval> Intervals { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ModerationShopSchedule() { }

    internal ModerationShopSchedule(
        DayOfWeek dayOfWeek,
        bool isClosed,
        IReadOnlyCollection<ModerationShopScheduleInterval> intervals)
    {
        DayOfWeek = dayOfWeek;
        IsClosed = isClosed;
        Intervals = intervals;
    }
}