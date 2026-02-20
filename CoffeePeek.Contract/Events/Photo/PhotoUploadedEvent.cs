namespace CoffeePeek.Contract.Events;

public record PhotoUploadedEvent(
    Guid PhotoId,
    string StorageKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string OwnerType,
    Guid OwnerId,
    DateTime UploadedAt
);