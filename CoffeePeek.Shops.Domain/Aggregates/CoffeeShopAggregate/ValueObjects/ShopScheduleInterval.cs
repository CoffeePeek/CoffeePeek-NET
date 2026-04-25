namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public record ShopScheduleInterval(TimeSpan OpenTime, TimeSpan CloseTime)
{
    public static ShopScheduleInterval Create(TimeSpan openTime, TimeSpan closeTime)
    {
        return new ShopScheduleInterval(openTime, closeTime);
    }
}