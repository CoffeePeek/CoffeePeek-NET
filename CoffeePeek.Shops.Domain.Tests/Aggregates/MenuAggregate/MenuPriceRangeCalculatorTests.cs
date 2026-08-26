using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;
using FluentAssertions;

namespace CoffeePeek.Shops.Domain.Tests.Aggregates.MenuAggregate;

public class MenuPriceRangeCalculatorTests
{
    [Fact]
    public void FromPrices_Empty_ReturnsNull()
    {
        MenuPriceRangeCalculator.FromPrices([]).Should().BeNull();
    }

    [Theory]
    [InlineData(6.5, PriceRange.Cheap)]
    [InlineData(8, PriceRange.Moderate)]
    [InlineData(10, PriceRange.Expensive)]
    public void FromPrices_AverageMapsToBand(double average, PriceRange expected)
    {
        MenuPriceRangeCalculator.FromPrices([(decimal)average]).Should().Be(expected);
    }
}
