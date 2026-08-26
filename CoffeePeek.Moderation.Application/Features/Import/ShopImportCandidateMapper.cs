using CoffeePeek.Contract.Dtos.Import;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using DomainBucket = CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate.ImportCollectorBucket;
using DomainFocus = CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate.ImportCoffeeFocus;
using DomainGoogle = CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate.ImportGoogleBusinessStatus;
using DomainStatus = CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate.ImportQueueStatus;
using DomainRejectReason = CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate.ImportRejectReason;
using ContractRejectReason = CoffeePeek.Contract.Enums.ImportRejectReason;

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
            candidate.CoffeeFocus is null ? null : (CoffeeShopType)(int)candidate.CoffeeFocus.Value,
            candidate.TagSlugs,
            candidate.GoogleBusinessStatus is null
                ? null
                : (GoogleBusinessStatus)(int)candidate.GoogleBusinessStatus.Value,
            candidate.GoogleFetchedAtUtc,
            candidate.GoogleBusinessStatus == DomainGoogle.ClosedPermanently,
            candidate.ReviewedByUserId,
            candidate.ReviewedAtUtc,
            candidate.ResultingShopId,
            candidate.RejectReason is null
                ? null
                : (ContractRejectReason)(int)candidate.RejectReason.Value,
            new ImportResearchLinksDto(
                links.Instagram,
                links.InstagramSearch,
                links.GoogleMaps,
                links.YandexMaps,
                links.YandexImages,
                links.OsmHistory),
            candidate.CreatedAtUtc,
            candidate.ImportedFromFile);
    }

    public static ImportDuplicateCandidateDto ToDuplicateDto(ShopImportCandidate candidate) =>
        new(
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
            candidate.QueueStatus.ToString(),
            candidate.ImportedFromFile,
            candidate.ResultingShopId);

    public static ImportDuplicateSuggestionDto ToDto(
        ShopImportDuplicateSuggestion suggestion,
        ShopImportCandidate left,
        ShopImportCandidate right) =>
        new(
            suggestion.Id,
            suggestion.Score,
            suggestion.DistanceMeters,
            suggestion.Reasons,
            suggestion.Status.ToString(),
            ToDuplicateDto(left),
            ToDuplicateDto(right),
            suggestion.ReviewedByUserId,
            suggestion.ReviewedAtUtc);

    public static DomainStatus ToDomain(Contract.Enums.ImportQueueStatus status) => (DomainStatus)(int)status;

    public static DomainFocus ToDomain(CoffeeShopType type) => (DomainFocus)(int)type;

    public static DomainBucket ToDomain(Contract.Enums.ImportCollectorBucket bucket) => (DomainBucket)(int)bucket;

    public static DomainRejectReason ToDomain(ContractRejectReason reason) =>
        (DomainRejectReason)(int)reason;
}
