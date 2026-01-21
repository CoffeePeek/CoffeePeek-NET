namespace CoffeePeek.Moderation.Domain.Entities;

public record ModerationShopScheduleInterval(TimeSpan OpenTime, TimeSpan CloseTime);