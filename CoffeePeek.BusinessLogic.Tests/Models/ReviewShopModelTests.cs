using CoffeePeek.BusinessLogic.Models;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Enums.Shop;
using FluentAssertions;

namespace CoffeePeek.BusinessLogic.Tests.Models;

public class ReviewShopModelTests
{
    [Fact]
    public void ReviewShopModel_Constructor_WithValidEntity_ShouldSetProperties()
    {
        // Arrange
        var entity = new ModerationShop
        {
            Id = 1,
            Name = "Test Shop"
        };

        // Act
        var model = new ReviewShopModel(entity);

        // Assert
        model.Should().NotBeNull();
        model.Id.Should().Be(1);
    }

    [Fact]
    public void ReviewShopModel_Constructor_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        ModerationShop entity = null!;

        // Act
        var action = () => new ReviewShopModel(entity);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("*entity*");
    }

    [Fact]
    public void ReviewShopModel_UpdateStatus_WithDifferentStatus_ShouldUpdateEntityStatus()
    {
        // Arrange
        var entity = new ModerationShop
        {
            Id = 1,
            ModerationStatus = ModerationStatus.Pending
        };
        var model = new ReviewShopModel(entity);
        var newStatus = ModerationStatus.Approved;

        // Act
        model.UpdateStatus(newStatus);

        // Assert
        entity.ModerationStatus.Should().Be(newStatus);
    }

    [Fact]
    public void ReviewShopModel_UpdateStatus_WithSameStatus_ShouldNotChangeEntityStatus()
    {
        // Arrange
        var initialStatus = ModerationStatus.Rejected;
        var entity = new ModerationShop
        {
            Id = 1,
            ModerationStatus = initialStatus
        };
        var model = new ReviewShopModel(entity);

        // Act
        model.UpdateStatus(initialStatus);

        // Assert
        entity.ModerationStatus.Should().Be(initialStatus);
    }

    [Fact]
    public void ReviewShopModel_Update_WithEntity_ShouldNotThrow()
    {
        // Arrange
        var entity = new ModerationShop
        {
            Id = 1,
            Name = "Original Shop"
        };
        var model = new ReviewShopModel(entity);
        var updatedEntity = new ModerationShop
        {
            Id = 1,
            Name = "Updated Shop"
        };

        // Act
        var action = () => model.Update(updatedEntity);

        // Assert
        action.Should().NotThrow();
    }
}