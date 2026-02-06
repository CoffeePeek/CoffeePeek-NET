using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities.CoffeeShopAggregate;
using FluentAssertions;
using JetBrains.Annotations;

namespace CoffeePeek.Shops.Domain.Tests.Entities.CoffeeShopAggregate;

[TestSubject(typeof(Equipment))]
public class EquipmentTest
{
    private readonly EquipmentCategory _testCategory = new();

    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        const string brand = "Sony";
        const string model = "Alpha A7 IV";

        // Act
        var equipment = new Equipment(brand, model, _testCategory, isCustom: true, isPrimary: true);

        // Assert
        equipment.Id.Should().NotBeEmpty();
        equipment.Brand.Should().Be(brand);
        equipment.ModelName.Should().Be(model);
        equipment.Name.Should().Be($"{brand} {model}");
        equipment.Category.Should().Be(_testCategory);
        equipment.IsCustom.Should().BeTrue();
        equipment.IsPrimary.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Model X")]
    [InlineData(" ", "Model X")]
    [InlineData(null, "Model X")]
    public void Constructor_ShouldThrowArgumentException_WhenBrandIsInvalid(string invalidBrand, string model)
    {
        // Act
        var act = () => { new Equipment(invalidBrand, model, _testCategory); };

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Brand is required");
    }

    [Theory]
    [InlineData("Sony", "")]
    [InlineData("Sony", " ")]
    [InlineData("Sony", null)]
    public void Constructor_ShouldThrowArgumentException_WhenModelNameIsInvalid(string brand, string invalidModel)
    {
        // Act
        Action act = () => new Equipment(brand, invalidModel, _testCategory);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Model name is required");
    }
    
    [Fact]
    public void MarkAsPrimary_ShouldSetIsPrimaryToTrue()
    {
        // Arrange
        var equipment = new Equipment("Brand", "Model", _testCategory, isPrimary: false);

        // Act
        equipment.MarkAsPrimary();

        // Assert
        equipment.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void UnmarkAsPrimary_ShouldSetIsPrimaryToFalse()
    {
        // Arrange
        var equipment = new Equipment("Brand", "Model", _testCategory, isPrimary: true);

        // Act
        equipment.UnmarkAsPrimary();

        // Assert
        equipment.IsPrimary.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetCustom_ShouldUpdateIsCustomValue(bool newValue)
    {
        // Arrange
        var equipment = new Equipment("Brand", "Model", _testCategory, isCustom: !newValue);

        // Act
        equipment.SetCustom(newValue);

        // Assert
        equipment.IsCustom.Should().Be(newValue);
    }
}