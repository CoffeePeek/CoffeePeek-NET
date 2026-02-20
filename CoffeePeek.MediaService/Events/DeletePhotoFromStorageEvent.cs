namespace CoffeePeek.MediaService.Events;

public record DeletePhotoFromStorageEvent(Guid PhotoId, string StorageKey);