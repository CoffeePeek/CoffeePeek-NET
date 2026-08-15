namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public enum ImportGoogleBusinessStatus
{
    Unknown = 0,
    Operational = 1,
    ClosedPermanently = 2,
    ClosedTemporarily = 3,
    NotFound = 4,
    Far = 5
}
