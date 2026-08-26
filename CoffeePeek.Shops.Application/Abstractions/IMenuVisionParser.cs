namespace CoffeePeek.Shops.Application.Abstractions;

public record VisionDrinkLine(string RawName, decimal? Price, int? VolumeMl, double? Confidence);

public record MenuVisionParseResult(
    bool Success,
    string? Error,
    IReadOnlyList<VisionDrinkLine> Drinks);

public interface IMenuVisionParser
{
    Task<MenuVisionParseResult> ParseAsync(
        IReadOnlyList<MenuVisionPhoto> photos,
        CancellationToken ct = default);
}

public record MenuVisionPhoto(byte[] Bytes, string ContentType);

public interface IMenuPhotoDownloader
{
    Task<IReadOnlyList<MenuVisionPhoto>> DownloadAsync(
        IReadOnlyList<MenuPhotoDownloadRequest> photos,
        CancellationToken ct = default);
}

public record MenuPhotoDownloadRequest(string Url, string ContentType);
