namespace Coffeepeek.Moderation.Application.Abstractions;

public interface IStorageService
{
    Task<(string UploadUrl, string StorageKey)> GetPresignedUploadUrlAsync(
        string fileName, 
        string contentType);
    
    Task MarkAsPermanentAsync(string storageKey);
}