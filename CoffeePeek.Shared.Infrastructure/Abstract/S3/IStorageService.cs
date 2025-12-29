namespace CoffeePeek.Shared.Infrastructure.Abstract.S3;

public interface IStorageService
{
    Task<(string UploadUrl, string StorageKey)> GetPresignedUploadUrlAsync(
        string fileName, 
        string contentType);
    
    Task MarkAsPermanentAsync(string storageKey);
    Task<bool> ExistsAsync(string uploadedPhotoStorageKey);
    Task DeleteAsync(string oldStorageKey);
}