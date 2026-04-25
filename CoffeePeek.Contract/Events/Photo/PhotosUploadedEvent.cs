namespace CoffeePeek.Contract.Events;

public record PhotosUploadedEvent(IEnumerable<PhotoUploadedEvent> Events);