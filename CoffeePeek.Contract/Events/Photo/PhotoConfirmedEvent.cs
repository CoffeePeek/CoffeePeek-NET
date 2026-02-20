namespace CoffeePeek.Contract.Events;

public record PhotoConfirmedEvent(
    Guid PhotoId,
    string StorageKey,
    string OwnerType,
    Guid OwnerId,
    Guid EntityId,
    DateTime ConfirmedAt
);