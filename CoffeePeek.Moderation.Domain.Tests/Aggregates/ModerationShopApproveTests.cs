using CoffeePeek.Moderation.Domain.Aggregates;
using CoffeePeek.Moderation.Domain.Common.Enums;
using CoffeePeek.Shared.Kernel.Exceptions;
using FluentAssertions;

namespace CoffeePeek.Moderation.Domain.Tests.Aggregates;

public class ModerationShopApproveTests
{
    private static ModerationShop CreatePendingShop() =>
        ModerationShop.Create("Test Cafe", Guid.NewGuid(), Guid.NewGuid(), description: null);

    [Fact]
    public void Approve_WhenLocationIsNull_ThrowsDomainException()
    {
        var shop = CreatePendingShop();

        var act = () => shop.Approve();

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot approve shop with unvalidated address.");
        shop.ModerationStatus.Should().Be(ModerationStatus.Pending);
    }

    [Fact]
    public void Approve_WhenLocationIsNotValidated_ThrowsDomainException()
    {
        var shop = CreatePendingShop();
        shop.SetLocation(new ModerationLocation("Unvalidated street"));

        var act = () => shop.Approve();

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot approve shop with unvalidated address.");
        shop.ModerationStatus.Should().Be(ModerationStatus.Pending);
    }

    [Fact]
    public void Approve_WhenLocationIsValidated_ReturnsTrueAndSetsApproved()
    {
        var shop = CreatePendingShop();
        shop.SetLocation(new ModerationLocation("Validated street", 53.9m, 27.5m));

        var changed = shop.Approve();

        changed.Should().BeTrue();
        shop.ModerationStatus.Should().Be(ModerationStatus.Approved);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ReturnsFalseWithoutThrowing()
    {
        var shop = CreatePendingShop();
        shop.SetLocation(new ModerationLocation("Validated street", 53.9m, 27.5m));
        shop.Approve();

        var changed = shop.Approve();

        changed.Should().BeFalse();
        shop.ModerationStatus.Should().Be(ModerationStatus.Approved);
    }

    [Fact]
    public void Create_StoresCoffeeFocus()
    {
        var shop = ModerationShop.Create(
            "Specialty Cafe",
            Guid.NewGuid(),
            Guid.NewGuid(),
            description: null,
            CoffeePeek.Moderation.Domain.Aggregates.Enums.CoffeeFocus.Specialty);

        shop.CoffeeFocus.Should().Be(CoffeePeek.Moderation.Domain.Aggregates.Enums.CoffeeFocus.Specialty);
    }

    [Fact]
    public void UpdateInfo_ChangesCoffeeFocus()
    {
        var shop = CreatePendingShop();

        shop.UpdateInfo(
            name: null,
            description: null,
            priceRange: null,
            cityId: null,
            CoffeePeek.Moderation.Domain.Aggregates.Enums.CoffeeFocus.CoffeeBar);

        shop.CoffeeFocus.Should().Be(CoffeePeek.Moderation.Domain.Aggregates.Enums.CoffeeFocus.CoffeeBar);
    }
}
