using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Contract.Dtos;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public interface IWebModerationShopsClient
{
    Task<Result<SendCoffeeShopToModerationResponseDto>> SendSuggestionAsync(
        SendCoffeeShopToModerationRequest request,
        CancellationToken ct = default);

    Task<Result<IReadOnlyList<UploadedPhotoDto>>> UploadShopPhotosAsync(
        IReadOnlyList<ShopPhotoBinaryPayload> photos,
        CancellationToken ct = default);
}
