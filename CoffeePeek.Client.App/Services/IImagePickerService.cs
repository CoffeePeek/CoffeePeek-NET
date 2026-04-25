namespace CoffeePeek.Client.App.Services;

public interface IImagePickerService
{
    Task<PickedImageFile?> PickImageAsync(CancellationToken ct = default);
}

public sealed record PickedImageFile(string FileName, string ContentType, byte[] Content);
