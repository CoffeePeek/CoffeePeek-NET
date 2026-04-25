using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Reviews;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Contract.Dtos;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebCoffeeShopReviewsClient(IHttpCommandExecutor httpCommandExecutor)
    : WebClientBase(httpCommandExecutor), IWebCoffeeShopReviewsClient
{
    public Task<Result<CanCreateCoffeeShopReviewResultDto>> CanCreateAsync(
        Guid shopId,
        CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Get)
            .WithEndpoint("api/CoffeeShopReviews/can-create")
            .WithQuery("shopId", shopId.ToString("D"))
            .Authorize();

        return Execute<CanCreateCoffeeShopReviewResultDto>(command, ct);
    }

    public Task<Result<CreateCoffeeShopReviewResultDto>> CreateAsync(
        Guid shopId,
        string? note,
        int placeScore,
        int serviceScore,
        int coffeeScore,
        CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Post)
            .WithEndpoint("api/CheckIns")
            .WithBody(new CreateCoffeeShopReviewRequest
            {
                CoffeeShopId = shopId,
                IsPublic = true,
                VisitedAt = DateTime.UtcNow,
                Note = note,
                Rating = new RatingDto
                {
                    Place = placeScore,
                    Service = serviceScore,
                    Coffee = coffeeScore
                }
            })
            .Authorize();

        return Execute<CreateCoffeeShopReviewResultDto>(command, ct);
    }

    public Task<Result> DeleteAsync(Guid reviewId, CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Delete)
            .WithEndpoint($"api/CoffeeShopReviews/{reviewId:D}")
            .Authorize();

        return Execute(command, ct);
    }
}
