namespace CoffeePeek.Contract.Events;

public record PhotoDeletedEvent(
    Guid PhotoId,
    string StorageKey,
    string OwnerType,
    Guid OwnerId,
    DateTime DeletedAt
);