using CoffeePeek.Contract.Dtos.Import;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using DomainBucket = CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate.ImportCollectorBucket;
using DomainFocus = CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate.ImportCoffeeFocus;
using DomainGoogle = CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate.ImportGoogleBusinessStatus;
using DomainStatus = CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate.ImportQueueStatus;

namespace CoffeePeek.Moderation.Application.Features.Import;

public static class ShopImportCandidateMapper
{
    public static ShopImportCandidateDto ToDto(ShopImportCandidate candidate)
    {
        var links = candidate.GetResearchLinks();
        return new ShopImportCandidateDto(
            candidate.Id,
            candidate.Source.ToString(),
            candidate.ExternalId,
            candidate.Name,
            candidate.Address,
            candidate.Latitude,
            candidate.Longitude,
            candidate.Phone,
            candidate.Website,
            candidate.Instagram,
            candidate.OpeningHours,
            candidate.Cuisine,
            candidate.Brand,
            candidate.OsmUpdatedAt,
            candidate.OsmAgeDays,
            candidate.CheckDate,
            candidate.Signals,
            (Contract.Enums.ImportCollectorBucket)(int)candidate.CollectorBucket,
            (Contract.Enums.ImportQueueStatus)(int)candidate.QueueStatus,
            candidate.CoffeeFocus is null ? null : (CoffeeFocus)(int)candidate.CoffeeFocus.Value,
            candidate.TagSlugs,
            candidate.GoogleBusinessStatus is null
                ? null
                : (GoogleBusinessStatus)(int)candidate.GoogleBusinessStatus.Value,
            candidate.GoogleFetchedAtUtc,
            candidate.GoogleBusinessStatus == DomainGoogle.ClosedPermanently,
            candidate.ReviewedByUserId,
            candidate.ReviewedAtUtc,
            candidate.ResultingShopId,
            new ImportResearchLinksDto(
                links.Instagram,
                links.InstagramSearch,
                links.GoogleMaps,
                links.YandexMaps,
                links.YandexImages,
                links.OsmHistory));
    }

    public static DomainStatus ToDomain(Contract.Enums.ImportQueueStatus status) => (DomainStatus)(int)status;

    public static DomainFocus ToDomain(CoffeeFocus focus) => (DomainFocus)(int)focus;

    public static DomainBucket ToDomain(Contract.Enums.ImportCollectorBucket bucket) => (DomainBucket)(int)bucket;
}
