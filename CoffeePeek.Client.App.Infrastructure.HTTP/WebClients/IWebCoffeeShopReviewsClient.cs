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
        CreateCoffeeShopReviewInput input,
        CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid reviewId, CancellationToken ct = default);
}

public sealed record CreateCoffeeShopReviewInput(
    int PlaceScore,
    int ServiceScore,
    int CoffeeScore,
    string? Note = null);
