using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public interface IWebCoffeeShopReviewsClient
{
    Task<Result<CanCreateCoffeeShopReviewResultDto>> CanCreateAsync(
        Guid shopId,
        CancellationToken ct = default);

    Task<Result<CreateCoffeeShopReviewResultDto>> CreateAsync(
        Guid shopId,
        string? note,
        int placeScore,
        int serviceScore,
        int coffeeScore,
        CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid reviewId, CancellationToken ct = default);
}
