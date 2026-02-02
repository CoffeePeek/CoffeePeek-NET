using CoffeePeek.Shops.Domain.Abstracts;

namespace CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

public record ShopScheduleInterval : ValueObjectBase
{
    public TimeSpan OpenTime { get; private set; }
    public TimeSpan CloseTime { get; private set; }

    private ShopScheduleInterval() { }

    protected ShopScheduleInterval(TimeSpan openTime, TimeSpan closeTime)
    {
        OpenTime = openTime;
        CloseTime = closeTime;
    }

    public static ShopScheduleInterval Create(TimeSpan openTime, TimeSpan closeTime)
    {
        return new ShopScheduleInterval(openTime, closeTime);
    }
}