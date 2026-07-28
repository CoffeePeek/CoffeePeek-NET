using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;
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
    public void AddPhotos_AssignsContiguousSortIndex()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var ownerId = Guid.NewGuid();

        shop.AddPhotos(
        [
            new ShopPhoto("a.jpg", "image/jpeg", "a", 1, ownerId),
            new ShopPhoto("b.jpg", "image/jpeg", "b", 1, ownerId),
        ]);

        shop.ShopPhotos.Select(p => p.SortIndex).Should().Equal(0, 1);
    }

    [Fact]
    public void ReorderPhotos_UpdatesSortIndexToMatchOrder()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var ownerId = Guid.NewGuid();
        var first = new ShopPhoto("a.jpg", "image/jpeg", "a", 1, ownerId);
        var second = new ShopPhoto("b.jpg", "image/jpeg", "b", 1, ownerId);
        var third = new ShopPhoto("c.jpg", "image/jpeg", "c", 1, ownerId);
        shop.AddPhotos([first, second, third]);

        shop.ReorderPhotos([third.Id, first.Id, second.Id]);

        shop.ShopPhotos.Single(p => p.Id == third.Id).SortIndex.Should().Be(0);
        shop.ShopPhotos.Single(p => p.Id == first.Id).SortIndex.Should().Be(1);
        shop.ShopPhotos.Single(p => p.Id == second.Id).SortIndex.Should().Be(2);
    }

    [Fact]
    public void ReorderPhotos_WithMissingId_Throws()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var photo = new ShopPhoto("a.jpg", "image/jpeg", "a", 1, Guid.NewGuid());
        shop.AddPhotos([photo]);

        var act = () => shop.ReorderPhotos([Guid.NewGuid()]);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ReorderPhotos_WithIncompleteList_Throws()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var ownerId = Guid.NewGuid();
        var first = new ShopPhoto("a.jpg", "image/jpeg", "a", 1, ownerId);
        var second = new ShopPhoto("b.jpg", "image/jpeg", "b", 1, ownerId);
        shop.AddPhotos([first, second]);

        var act = () => shop.ReorderPhotos([first.Id]);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddEquipment_WithDuplicateBrandAndModel_DoesNotAddTwice()
    {
        var shop = new CoffeeShop(Guid.NewGuid(), "Test Shop", null, PriceRange.Cheap, Guid.NewGuid());
        var category = new EquipmentCategory();
        var e1 = new Equipment("Brand", "Model", category);
        var e2 = new Equipment("Brand", "Model", category);

        shop.AddEquipment(e1);
        shop.AddEquipment(e2);

        shop.Equipments.Count.Should().Be(1);
    }
}
