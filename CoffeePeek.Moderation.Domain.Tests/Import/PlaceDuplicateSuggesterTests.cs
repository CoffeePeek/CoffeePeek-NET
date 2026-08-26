using CoffeePeek.Shared.Domain.Places;
using FluentAssertions;

namespace CoffeePeek.Moderation.Domain.Tests.Import;

public class PlaceDuplicateSuggesterTests
{
    private const decimal Lat = 53.9045m;
    private const decimal Lon = 27.5615m;

    [Fact]
    public void SurfCoffee_BelarusianVsRussianAddress_IsSuggested()
    {
        var hint = PlaceDuplicateSuggester.Evaluate(
            "Surf Coffee",
            "пр. Незалежнасці, 25, Мінск",
            Lat,
            Lon,
            "Surf Coffee",
            "проспект Независимости, 25, Минск",
            Lat + 0.0010m,
            Lon + 0.0012m);

        hint.Should().NotBeNull();
        hint!.Value.Score.Should().BeGreaterThanOrEqualTo(PlaceDuplicateSuggester.MinScore);
        hint.Value.Reasons.Should().Contain("same-name");
        hint.Value.Reasons.Should().Contain("same-house-nearby");
        hint.Value.Reasons.Should().Contain("similar-address");
    }

    [Fact]
    public void SameChain_FarApart_IsNotSuggested()
    {
        var hint = PlaceDuplicateSuggester.Evaluate(
            "Surf Coffee",
            "пр. Незалежнасці, 25",
            Lat,
            Lon,
            "Surf Coffee",
            "вул. Няміга, 5",
            53.90m,
            27.50m);

        hint.Should().BeNull();
    }

    [Fact]
    public void DifferentNamesNearby_IsNotSuggested()
    {
        var hint = PlaceDuplicateSuggester.Evaluate(
            "Coffe Joy",
            "Немига 5",
            Lat,
            Lon,
            "Kitchen Coffee Roasters",
            "Немига 7",
            Lat + 0.0002m,
            Lon + 0.0002m);

        hint.Should().BeNull();
    }

    [Fact]
    public void FoldEastSlavic_MapsBelarusianLetters()
    {
        PlaceDuplicateSuggester.FoldEastSlavic("Незалежнасці").Should().Be("Незалежнасци");
        PlaceDuplicateSuggester.HouseNumber("пр. Незалежнасці, 25а").Should().Be("25а");
        PlaceDuplicateSuggester.NormalizeAddress("проспект Независимости, 25, Минск")
            .Should().Contain("пр")
            .And.Contain("25");
    }
}
