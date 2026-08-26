using CoffeePeek.Shops.Application.Abstractions;

namespace CoffeePeek.Shops.Infrastructure.Menu;

public sealed class HttpMenuPhotoDownloader(IHttpClientFactory httpClientFactory) : IMenuPhotoDownloader
{
    public async Task<IReadOnlyList<MenuVisionPhoto>> DownloadAsync(
        IReadOnlyList<MenuPhotoDownloadRequest> photos,
        CancellationToken ct = default)
    {
        var http = httpClientFactory.CreateClient("menu-photos");
        var downloaded = new List<MenuVisionPhoto>();

        foreach (var photo in photos)
        {
            using var response = await http.GetAsync(photo.Url, ct);
            if (!response.IsSuccessStatusCode)
                continue;

            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            if (bytes.Length == 0)
                continue;

            downloaded.Add(new MenuVisionPhoto(
                bytes,
                string.IsNullOrWhiteSpace(photo.ContentType) ? "image/jpeg" : photo.ContentType));
        }

        return downloaded;
    }
}
