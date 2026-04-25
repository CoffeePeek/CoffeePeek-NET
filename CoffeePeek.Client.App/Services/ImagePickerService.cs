using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace CoffeePeek.Client.App.Services;

public sealed class ImagePickerService : IImagePickerService
{
    private static readonly FilePickerFileType ImagePickerType = new("Images")
    {
        Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.gif"]
    };

    public async Task<PickedImageFile?> PickImageAsync(CancellationToken ct = default)
    {
        var topLevel = ResolveTopLevel();
        if (topLevel?.StorageProvider is null)
            return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [ImagePickerType]
        });

        var file = files.FirstOrDefault();
        if (file is null)
            return null;

        await using var stream = await file.OpenReadAsync();
        await using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);

        var fileName = file.Name ?? "photo";
        var contentType = GetContentTypeFromExtension(fileName);
        return new PickedImageFile(fileName, contentType, ms.ToArray());
    }

    private static TopLevel? ResolveTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView
            && singleView.MainView is Visual visual)
            return TopLevel.GetTopLevel(visual);

        return null;
    }

    private static string GetContentTypeFromExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
