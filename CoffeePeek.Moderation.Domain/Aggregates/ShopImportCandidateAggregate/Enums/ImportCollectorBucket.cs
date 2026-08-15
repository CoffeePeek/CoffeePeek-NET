namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public enum ImportCollectorBucket
{
    Priority = 0,
    Review = 1,
    LikelyNoise = 2,
    AutoReject = 3,
    Stale = 4,
    LikelySpecialty = 5
}
