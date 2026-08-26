using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;

namespace CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;

public static class MenuPriceRangeCalculator
{
    public static PriceRange? FromPrices(
        IEnumerable<decimal> prices,
        decimal cheapBelow = BusinessConstants.MenuCheapBelow,
        decimal expensiveAbove = BusinessConstants.MenuExpensiveAbove)
    {
        var values = prices.Where(p => p > 0).ToArray();
        if (values.Length == 0)
            return null;

        var average = values.Average();
        if (average < cheapBelow)
            return PriceRange.Cheap;
        if (average > expensiveAbove)
            return PriceRange.Expensive;
        return PriceRange.Moderate;
    }
}
