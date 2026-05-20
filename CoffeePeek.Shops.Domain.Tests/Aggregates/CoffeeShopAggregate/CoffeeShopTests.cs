using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using FluentAssertions;
using JetBrains.Annotations;

namespace CoffeePeek.Shops.Domain.Tests.Aggregates.CoffeeShopAggregate;

[TestSubject(typeof(CoffeeShop))]
public class CoffeeShopTests
{
    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var moderationId = Guid.NewGuid();

        // Act
        var shop = new CoffeeShop(creatorId, "Test Shop", null, PriceRange.Moderate, moderationId);

        // Assert
        shop.Name.Should().Be("Test Shop");
        shop.CreatorId.Should().Be(creatorId);
        shop.Id.Should().NotBeEmpty();
        shop.Status.Should().Be(CoffeeShopStatus.Active);
    }

    [Fact]
    public void IsOpen_WhenActiveWithNoSchedule_ReturnsTrue()
    {
        // Arrange — Active shop with no schedules returns true from IsOpenAt
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());

        // Act & Assert
        shop.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void AddEquipment_WithDuplicateBrandAndModel_DoesNotAddTwice()
    {
        // Arrange
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var category = new EquipmentCategory();
        var e1 = new Equipment("Brand", "Model", category);
        var e2 = new Equipment("Brand", "Model", category);

        // Act
        shop.AddEquipment(e1);
        shop.AddEquipment(e2);

        // Assert — duplicate should be ignored
        shop.Equipments.Count.Should().Be(1);
    }
}
