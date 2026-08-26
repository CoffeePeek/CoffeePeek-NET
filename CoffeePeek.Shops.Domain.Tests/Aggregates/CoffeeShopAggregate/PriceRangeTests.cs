using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using FluentAssertions;

namespace CoffeePeek.Shops.Domain.Tests.Aggregates.CoffeeShopAggregate;

public class PriceRangeTests
{
    [Fact]
    public void HasExactlyThreeLevels_CheapModerateExpensive()
    {
        Enum.GetValues<PriceRange>().Should().Equal(
            PriceRange.Cheap,
            PriceRange.Moderate,
            PriceRange.Expensive);

        ((int)PriceRange.Cheap).Should().Be(1);
        ((int)PriceRange.Moderate).Should().Be(2);
        ((int)PriceRange.Expensive).Should().Be(3);
    }
}
