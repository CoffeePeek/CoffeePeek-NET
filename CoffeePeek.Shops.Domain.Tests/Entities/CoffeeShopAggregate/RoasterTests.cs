using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using FluentAssertions;

namespace CoffeePeek.Shops.Domain.Tests.Entities.CoffeeShopAggregate;

public class RoasterTests
{
    [Fact]
    public void Constructor_ValidName_SetsIdAndName()
    {
        var roaster = new Roaster("Coffee Circus");

        roaster.Id.Should().NotBeEmpty();
        roaster.Name.Should().Be("Coffee Circus");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_BlankName_Throws(string invalidName)
    {
        var act = () => new Roaster(invalidName);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_NameTooLong_Throws()
    {
        var name = new string('a', BusinessConstants.MaxRoasterNameLength + 1);

        var act = () => new Roaster(name);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_ValidName_ChangesNameKeepsId()
    {
        var roaster = new Roaster("Coffee Circus");
        var originalId = roaster.Id;

        roaster.Update("Grunwald Coffee Roasters");

        roaster.Id.Should().Be(originalId);
        roaster.Name.Should().Be("Grunwald Coffee Roasters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_BlankName_Throws(string invalidName)
    {
        var roaster = new Roaster("Coffee Circus");

        var act = () => roaster.Update(invalidName);

        act.Should().Throw<DomainException>();
        roaster.Name.Should().Be("Coffee Circus");
    }

    [Fact]
    public void Update_NameTooLong_Throws()
    {
        var roaster = new Roaster("Coffee Circus");
        var name = new string('a', BusinessConstants.MaxRoasterNameLength + 1);

        var act = () => roaster.Update(name);

        act.Should().Throw<DomainException>();
        roaster.Name.Should().Be("Coffee Circus");
    }
}
