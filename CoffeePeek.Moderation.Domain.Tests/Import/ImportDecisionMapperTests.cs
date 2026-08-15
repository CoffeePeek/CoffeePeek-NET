using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Moderation.Domain.Import;
using FluentAssertions;

namespace CoffeePeek.Moderation.Domain.Tests.Import;

public class ImportDecisionMapperTests
{
    [Theory]
    [InlineData("yes", ImportQueueStatus.Published, ImportCoffeeFocus.Specialty)]
    [InlineData("specialty", ImportQueueStatus.Published, ImportCoffeeFocus.Specialty)]
    [InlineData("good_coffee", ImportQueueStatus.Published, ImportCoffeeFocus.CoffeeBar)]
    [InlineData("cafe", ImportQueueStatus.Published, ImportCoffeeFocus.Cafe)]
    public void FromSpike_QualityKeys_MapToFocusAndPublished(
        string raw,
        ImportQueueStatus status,
        ImportCoffeeFocus focus)
    {
        var mapped = ImportDecisionMapper.FromSpike(raw);

        mapped.Should().NotBeNull();
        mapped!.Value.Status.Should().Be(status);
        mapped.Value.Focus.Should().Be(focus);
    }

    [Fact]
    public void FromSpike_ToGo_PublishesWithTag()
    {
        var mapped = ImportDecisionMapper.FromSpike("to_go");

        mapped.Should().NotBeNull();
        mapped!.Value.Status.Should().Be(ImportQueueStatus.Published);
        mapped.Value.TagSlugs.Should().Contain("to_go");
    }

    [Theory]
    [InlineData("no")]
    [InlineData("reject")]
    [InlineData("invalid")]
    public void FromSpike_RejectKeys_MapToRejectedInvalid(string raw)
    {
        var mapped = ImportDecisionMapper.FromSpike(raw);

        mapped.Should().NotBeNull();
        mapped!.Value.Status.Should().Be(ImportQueueStatus.Rejected);
        mapped.Value.Focus.Should().BeNull();
        mapped.Value.RejectReason.Should().Be(ImportRejectReason.Invalid);
    }

    [Theory]
    [InlineData("closed", ImportRejectReason.Closed)]
    [InlineData("not_coffee", ImportRejectReason.NotCoffee)]
    [InlineData("notcoffee", ImportRejectReason.NotCoffee)]
    public void FromSpike_TypedRejectKeys_MapReason(string raw, ImportRejectReason reason)
    {
        var mapped = ImportDecisionMapper.FromSpike(raw);

        mapped.Should().NotBeNull();
        mapped!.Value.Status.Should().Be(ImportQueueStatus.Rejected);
        mapped.Value.RejectReason.Should().Be(reason);
    }

    [Theory]
    [InlineData("skip")]
    [InlineData("later")]
    public void FromSpike_SkipKeys_MapToSkipped(string raw)
    {
        ImportDecisionMapper.FromSpike(raw)!.Value.Status.Should().Be(ImportQueueStatus.Skipped);
    }

    [Fact]
    public void FromSpike_Unknown_ReturnsNull()
    {
        ImportDecisionMapper.FromSpike("maybe").Should().BeNull();
    }
}
