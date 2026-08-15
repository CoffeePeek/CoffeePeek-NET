using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Moderation.Domain.Import;
using FluentAssertions;

namespace CoffeePeek.Moderation.Domain.Tests.Import;

public class OsmCafeClassifierTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Classify_StaleOverFiveYears_IsStaleBucket()
    {
        var tags = new Dictionary<string, string> { ["amenity"] = "cafe", ["name"] = "Coffe Joy" };
        var updated = Now.AddYears(-6);

        var (bucket, signals, stale) = OsmCafeClassifier.Classify(tags, updated, Now);

        stale.Should().BeTrue();
        bucket.Should().Be(ImportCollectorBucket.Stale);
        signals.Should().Contain(s => s.StartsWith("osm:stale"));
    }

    [Fact]
    public void Classify_RecentCoffeeName_IsPriority()
    {
        var tags = new Dictionary<string, string> { ["amenity"] = "cafe", ["name"] = "Coffee Joy" };

        var (bucket, _, stale) = OsmCafeClassifier.Classify(tags, Now.AddMonths(-2), Now);

        stale.Should().BeFalse();
        bucket.Should().Be(ImportCollectorBucket.Priority);
    }

    [Fact]
    public void Classify_GenericCafe_IsReview()
    {
        var tags = new Dictionary<string, string> { ["amenity"] = "cafe", ["name"] = "Лідо" };

        var (bucket, _, _) = OsmCafeClassifier.Classify(tags, Now.AddMonths(-2), Now);

        bucket.Should().Be(ImportCollectorBucket.Review);
    }
}
