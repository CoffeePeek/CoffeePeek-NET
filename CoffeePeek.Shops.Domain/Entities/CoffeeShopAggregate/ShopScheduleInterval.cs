namespace CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;

public record ShopScheduleInterval(TimeSpan OpenTime, TimeSpan CloseTime);