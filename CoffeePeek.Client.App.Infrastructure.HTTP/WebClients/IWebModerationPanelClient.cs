using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public interface IWebModerationPanelClient
{
    Task<Result<ModerationShopDto[]>> GetAllShopsAsync(CancellationToken ct = default);

    Task<Result<ModerationReviewDto[]>> GetAllReviewsAsync(CancellationToken ct = default);

    Task<Result> UpdateShopStatusAsync(
        Guid shopId,
        ModerationStatus status,
        CancellationToken ct = default);

    Task<Result> UpdateReviewStatusAsync(
        Guid reviewId,
        ModerationStatus status,
        string? rejectReason,
        CancellationToken ct = default);
}
