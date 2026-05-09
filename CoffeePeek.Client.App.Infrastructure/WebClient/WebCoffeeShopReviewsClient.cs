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
        CreateCoffeeShopReviewInput input,
        CancellationToken ct = default)
    {
        if (!IsValidScore(input.PlaceScore) || !IsValidScore(input.ServiceScore) || !IsValidScore(input.CoffeeScore))
            return Task.FromResult(Result.Fail<CreateCoffeeShopReviewResultDto>("Review scores must be between 1 and 5."));

        var command = new HttpCommand()
            .WithMethod(HttpMethod.Post)
            .WithEndpoint("api/CheckIns")
            .WithBody(new CreateCoffeeShopReviewRequest
            {
                CoffeeShopId = shopId,
                // Current UI supports only public, immediate post-visit reviews. If private check-ins
                // or back-dated reviews are added later, parameterize these fields in CreateAsync.
                IsPublic = true,
                VisitedAt = DateTime.UtcNow,
                Note = input.Note,
                Rating = new RatingDto
                {
                    Place = input.PlaceScore,
                    Service = input.ServiceScore,
                    Coffee = input.CoffeeScore
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

    private static bool IsValidScore(int score) => score is >= 1 and <= 5;
}
