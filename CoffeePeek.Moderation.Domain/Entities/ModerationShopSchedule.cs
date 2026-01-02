using CoffeePeek.Contract.Dtos.Schedule;

namespace CoffeePeek.Moderation.Domain.Entities;

public sealed class ModerationShopSchedule : Entity<Guid>
{
    public Guid ModerationShopId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsClosed { get; private set; }

    public ModerationShop ModerationShop { get; private set; }
    private readonly List<ModerationShopScheduleInterval> _intervals = [];
    public IReadOnlyCollection<ModerationShopScheduleInterval> Intervals => _intervals.AsReadOnly();

    // ReSharper disable once UnusedMember.Local
    private ModerationShopSchedule() { }

    internal ModerationShopSchedule(DayOfWeek dayOfWeek, List<ShopScheduleIntervalDto>? intervals)
    {
        Id = Guid.NewGuid();
        DayOfWeek = dayOfWeek;
        
        if (intervals != null)
            _intervals = intervals.Select(x => 
                new ModerationShopScheduleInterval(x.OpenTime, x.CloseTime)
            ).ToList();
    }
}