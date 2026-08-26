using CoffeePeek.Shops.Domain.Aggregates.MenuAggregate;
using FluentAssertions;

namespace CoffeePeek.Shops.Domain.Tests.Aggregates.MenuAggregate;

public class MenuDrinkMatcherTests
{
    private static IReadOnlyList<CoffeeDrinkDefinition> Catalog() =>
    [
        CoffeeDrinkDefinition.CreateWithId(
            CoffeeDrinkIds.Espresso, "espresso", "Эспрессо", "Espresso",
            CoffeeDrinkCategory.Espresso, "эспрессо,espresso", 10),
        CoffeeDrinkDefinition.CreateWithId(
            CoffeeDrinkIds.Doppio, "doppio", "Доппио", "Doppio",
            CoffeeDrinkCategory.Espresso, "доппио,doppio,double espresso,двойной эспрессо", 20),
        CoffeeDrinkDefinition.CreateWithId(
            CoffeeDrinkIds.Cappuccino, "cappuccino", "Капучино", "Cappuccino",
            CoffeeDrinkCategory.Espresso, "капучино,капуч,cappuccino,cappucino", 40),
        CoffeeDrinkDefinition.CreateWithId(
            CoffeeDrinkIds.V60, "v60", "V60 / воронка", "V60",
            CoffeeDrinkCategory.Filter, "v60,v-60,воронка,hario,pour over", 110)
    ];

    [Theory]
    [InlineData("Воронка", "v60")]
    [InlineData("доппио", "doppio")]
    [InlineData("Капучино 250 мл", "cappuccino")]
    [InlineData("двойной эспрессо", "doppio")]
    public void Match_KnownAliases_ReturnsCatalogSlug(string raw, string slug)
    {
        MenuDrinkMatcher.Match(Catalog(), raw)!.Slug.Should().Be(slug);
    }

    [Fact]
    public void Match_UnknownDrink_ReturnsNull()
    {
        MenuDrinkMatcher.Match(Catalog(), "Матча латте").Should().BeNull();
    }
}
