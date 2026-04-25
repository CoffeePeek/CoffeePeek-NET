namespace CoffeePeek.Contract.Events;

public record PhotoReplacedEvent(
    Guid OldPhotoId,
    string OldStorageKey,
    Guid NewPhotoId,
    string OwnerType,
    Guid OwnerId,
    DateTime ReplacedAt
);