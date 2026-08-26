using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Moderation.Domain.Import;
using FluentAssertions;

namespace CoffeePeek.Moderation.Domain.Tests.Import;

public class CoffeeMapClassifierTests
{
    [Fact]
    public void Specialty_GoesToLikelySpecialty()
    {
        var (bucket, signals) = CoffeeMapClassifier.Classify(Snapshot(isSpecialty: true));
        bucket.Should().Be(ImportCollectorBucket.LikelySpecialty);
        signals.Should().Contain("coffeemap:specialty");
    }

    [Fact]
    public void Recommended_GoesToPriority()
    {
        var (bucket, _) = CoffeeMapClassifier.Classify(Snapshot(recommended: true));
        bucket.Should().Be(ImportCollectorBucket.Priority);
    }

    [Fact]
    public void CoffeeName_GoesToPriority()
    {
        var (bucket, signals) = CoffeeMapClassifier.Classify(Snapshot(name: "Lavazza coffee"));
        bucket.Should().Be(ImportCollectorBucket.Priority);
        signals.Should().Contain("name:coffee");
    }

    private static CoffeeMapCandidateSnapshot Snapshot(
        string name = "Cafe",
        bool isSpecialty = false,
        bool recommended = false) =>
        new(
            "1",
            name,
            "Минск",
            53.9m,
            27.5m,
            null,
            null,
            null,
            null,
            null,
            isSpecialty,
            recommended,
            null,
            null,
            null,
            []);
}
