using CoffeePeek.Shops.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Shops.Infrastructure.Menu;

public sealed class HttpMenuPhotoDownloader(
    IHttpClientFactory httpClientFactory,
    ILogger<HttpMenuPhotoDownloader> logger) : IMenuPhotoDownloader
{
    public async Task<IReadOnlyList<MenuVisionPhoto>> DownloadAsync(
        IReadOnlyList<MenuPhotoDownloadRequest> photos,
        CancellationToken ct = default)
    {
        var http = httpClientFactory.CreateClient("menu-photos");
        var downloaded = new List<MenuVisionPhoto>();

        foreach (var photo in photos)
        {
            try
            {
                using var response = await http.GetAsync(photo.Url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Menu photo download HTTP {StatusCode} for {Url}",
                        (int)response.StatusCode,
                        photo.Url);
                    continue;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                if (bytes.Length == 0)
                {
                    logger.LogWarning("Menu photo download was empty for {Url}", photo.Url);
                    continue;
                }

                downloaded.Add(new MenuVisionPhoto(
                    bytes,
                    string.IsNullOrWhiteSpace(photo.ContentType) ? "image/jpeg" : photo.ContentType));
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Menu photo download threw for {Url}", photo.Url);
            }
        }

        if (photos.Count > 0 && downloaded.Count == 0)
            logger.LogError("Could not download any of {Count} menu photos", photos.Count);

        return downloaded;
    }
}
