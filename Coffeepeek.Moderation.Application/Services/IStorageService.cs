namespace CoffeePeek.Moderation.Application.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream stream, string contentType, CancellationToken ct);
    Task DeleteFileAsync(string fileKey, CancellationToken ct);
    string GetFileUrl(string fileKey);
}