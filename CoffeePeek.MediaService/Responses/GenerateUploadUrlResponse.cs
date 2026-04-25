namespace CoffeePeek.MediaService.Responses;

public record GenerateUploadUrlResponse(Guid PhotoId, string UploadUrl, string StorageKey);