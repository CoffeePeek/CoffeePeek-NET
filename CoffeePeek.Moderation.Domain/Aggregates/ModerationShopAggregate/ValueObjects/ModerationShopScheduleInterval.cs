namespace CoffeePeek.Moderation.Domain.Aggregates;

public record ModerationShopScheduleInterval(TimeSpan OpenTime, TimeSpan CloseTime);