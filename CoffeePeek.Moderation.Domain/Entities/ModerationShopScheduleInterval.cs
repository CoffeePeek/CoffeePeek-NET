using CoffeePeek.Shared.Extensions.Exceptions;

namespace CoffeePeek.Moderation.Domain.Entities;

public sealed class ModerationShopScheduleInterval : Entity<Guid>
{
    public Guid ModerationShopScheduleId { get; private set; }
    public TimeSpan OpenTime { get; private set; }
    public TimeSpan CloseTime { get; private set; }

    public ModerationShopSchedule Schedule { get; private set; }

    private ModerationShopScheduleInterval() { }

    internal ModerationShopScheduleInterval(TimeSpan openTime, TimeSpan closeTime)
    {
        if (openTime >= closeTime)
            throw new DomainException("Время открытия должно быть раньше времени закрытия.");

        Id = Guid.NewGuid();
        OpenTime = openTime;
        CloseTime = closeTime;
    }
}