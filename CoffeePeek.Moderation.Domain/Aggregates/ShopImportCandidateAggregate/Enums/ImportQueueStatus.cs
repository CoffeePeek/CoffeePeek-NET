namespace CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;

public enum ImportQueueStatus
{
    Pending = 0,
    Skipped = 1,
    Published = 2,
    Rejected = 3
}
