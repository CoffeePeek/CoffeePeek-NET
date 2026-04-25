using System.Net.Http.Headers;
using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Contract.Dtos;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebModerationShopsClient(
    IHttpCommandExecutor httpCommandExecutor,
    HttpClient httpClient) : WebClientBase(httpCommandExecutor), IWebModerationShopsClient
{
    public Task<Result<SendCoffeeShopToModerationResponseDto>> SendSuggestionAsync(
        SendCoffeeShopToModerationRequest request,
        CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Post)
            .WithEndpoint("api/ModerationShops")
            .WithBody(request)
            .Authorize();

        return Execute<SendCoffeeShopToModerationResponseDto>(command, ct);
    }

    public async Task<Result<IReadOnlyList<UploadedPhotoDto>>> UploadShopPhotosAsync(
        IReadOnlyList<ShopPhotoBinaryPayload> photos,
        CancellationToken ct = default)
    {
        if (photos.Count == 0)
            return Result.Ok<IReadOnlyList<UploadedPhotoDto>>([]);

        var uploadRequest = photos
            .Select(p => new ShopPhotoUploadRequest
            {
                SizeBytes = p.Content.Length,
                FileName = p.FileName,
                ContentType = string.IsNullOrWhiteSpace(p.ContentType) ? "application/octet-stream" : p.ContentType
            })
            .ToList();

        var command = new HttpCommand()
            .WithMethod(HttpMethod.Post)
            .WithEndpoint("api/Photos/shop")
            .WithBody(uploadRequest)
            .Authorize();

        var urlResult = await Execute<List<GenerateUploadUrlResponseDto>>(command, ct);
        if (urlResult.IsFailed)
            return Result.Fail(urlResult.Errors);

        var urls = urlResult.Value;
        if (urls.Count != photos.Count)
            return Result.Fail("Photo upload initialization returned unexpected number of URLs.");

        var uploaded = new List<UploadedPhotoDto>(photos.Count);

        for (var i = 0; i < photos.Count; i++)
        {
            var file = photos[i];
            var uploadMeta = urls[i];
            var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;

            using var putRequest = new HttpRequestMessage(HttpMethod.Put, uploadMeta.UploadUrl)
            {
                Content = new ByteArrayContent(file.Content)
            };
            putRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            using var putResponse = await httpClient.SendAsync(putRequest, ct);
            if (!putResponse.IsSuccessStatusCode)
                return Result.Fail($"Photo upload failed with status {(int)putResponse.StatusCode}.");

            uploaded.Add(new UploadedPhotoDto(
                file.FileName,
                contentType,
                uploadMeta.StorageKey,
                file.Content.LongLength));
        }

        return Result.Ok<IReadOnlyList<UploadedPhotoDto>>(uploaded);
    }
}
