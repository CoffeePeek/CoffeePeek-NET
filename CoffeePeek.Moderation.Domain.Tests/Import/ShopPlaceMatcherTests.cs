using CoffeePeek.Shared.Domain.Places;
using FluentAssertions;

namespace CoffeePeek.Moderation.Domain.Tests.Import;

public class ShopPlaceMatcherTests
{
    [Fact]
    public void SameNameAndNearby_IsDuplicate()
    {
        ShopPlaceMatcher.IsSamePlace(
            "Coffe Joy", 53.9152m, 27.5847m,
            "Coffe Joy", 53.9153m, 27.5848m).Should().BeTrue();
    }

    [Fact]
    public void CloseSpellingNearby_IsDuplicate()
    {
        ShopPlaceMatcher.IsSamePlace(
            "Coffe Joy", 53.9152m, 27.5847m,
            "Coffee Joy", 53.91525m, 27.58475m).Should().BeTrue();
    }

    [Fact]
    public void SameNameFarAway_IsNotDuplicate()
    {
        ShopPlaceMatcher.IsSamePlace(
            "Varka", 53.9152m, 27.5847m,
            "Varka", 53.90m, 27.50m).Should().BeFalse();
    }

    [Fact]
    public void DifferentNameNearby_IsNotDuplicate()
    {
        ShopPlaceMatcher.IsSamePlace(
            "Coffe Joy", 53.9152m, 27.5847m,
            "Kitchen Coffee Roasters", 53.9153m, 27.5848m).Should().BeFalse();
    }

    [Fact]
    public void SameInstagram_MatchesEvenIfNamesDiffer()
    {
        ShopPlaceMatcher.IsSamePlace(
            "Joy", 53.9152m, 27.5847m,
            "CoffeJoy Bar", 53.9160m, 27.5860m,
            instagramA: "https://instagram.com/coffejoy",
            instagramB: "@coffejoy").Should().BeTrue();
    }

    [Fact]
    public void SamePhone_Matches()
    {
        ShopPlaceMatcher.IsSamePlace(
            "A", 53.9152m, 27.5847m,
            "B", 53.9160m, 27.5860m,
            phoneA: "+375 29 111-22-33",
            phoneB: "375291112233").Should().BeTrue();
    }

    [Fact]
    public void PreferRicherText_FillsEmptyAndKeepsExisting()
    {
        ShopPlaceMatcher.PreferRicherText(null, "+375 29 111-22-33").Should().Be("+375 29 111-22-33");
        ShopPlaceMatcher.PreferRicherText("+375 29 111-22-33", null).Should().Be("+375 29 111-22-33");
        ShopPlaceMatcher.PreferRicherText("Минск", "Немига 5, Минск").Should().Be("Немига 5, Минск");
    }
}
