namespace CoffeePeek.Shops.Domain.Entities;

public record ShopScheduleInterval(TimeSpan OpenTime, TimeSpan CloseTime);