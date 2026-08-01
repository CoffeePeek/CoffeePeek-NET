using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain;
using CoffeePeek.Shops.Domain.Aggregates.ShopTagAggregate;
using FluentAssertions;

namespace CoffeePeek.Shops.Domain.Tests.Aggregates.ShopTagAggregate;

public class ShopTagTests
{
    [Fact]
    public void Create_NormalizesSlugAndSetsActive()
    {
        var tag = ShopTag.Create(" Laptop Friendly ", "Laptop Friendly", "Good for work", 10);

        tag.Slug.Should().Be("laptop_friendly");
        tag.Name.Should().Be("Laptop Friendly");
        tag.Description.Should().Be("Good for work");
        tag.SortOrder.Should().Be(10);
        tag.IsActive.Should().BeTrue();
        tag.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_EmptySlug_Throws()
    {
        var act = () => ShopTag.Create("  ", "Name");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_ChangesMutableFields_NotSlug()
    {
        var tag = ShopTag.Create("specialty", "Specialty");
        var originalSlug = tag.Slug;

        tag.Update("Specialty Coffee", "Beans focus", 5, false);

        tag.Slug.Should().Be(originalSlug);
        tag.Name.Should().Be("Specialty Coffee");
        tag.Description.Should().Be("Beans focus");
        tag.SortOrder.Should().Be(5);
        tag.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var tag = ShopTag.Create("quiet_work", "Quiet Work");
        tag.Deactivate();
        tag.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Create_NameTooLong_Throws()
    {
        var name = new string('a', BusinessConstants.MaxShopTagNameLength + 1);
        var act = () => ShopTag.Create("slug", name);
        act.Should().Throw<DomainException>();
    }
}
