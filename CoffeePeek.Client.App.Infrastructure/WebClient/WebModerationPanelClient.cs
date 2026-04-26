using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Enums;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebModerationPanelClient(IHttpCommandExecutor httpCommandExecutor)
    : WebClientBase(httpCommandExecutor), IWebModerationPanelClient
{
    public async Task<Result<ModerationShopDto[]>> GetAllShopsAsync(CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Get)
            .WithEndpoint("api/ModerationShops")
            .Authorize();

        var result = await Execute<GetAllModerationShopsResultDto>(command, ct);
        if (result.IsFailed)
            return Result.Fail(result.Errors);

        return Result.Ok(result.Value.ModerationShop);
    }

    public async Task<Result<ModerationReviewDto[]>> GetAllReviewsAsync(CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Get)
            .WithEndpoint("api/ModerationReviews")
            .Authorize();

        var result = await Execute<GetAllModerationReviewsResultDto>(command, ct);
        if (result.IsFailed)
            return Result.Fail(result.Errors);

        return Result.Ok(result.Value.Reviews);
    }

    public Task<Result> UpdateShopStatusAsync(
        Guid shopId,
        ModerationStatus status,
        CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Put)
            .WithEndpoint("api/ModerationShops/status")
            .WithQuery("id", shopId.ToString("D"))
            .WithQuery("status", status.ToString())
            .Authorize();

        return Execute(command, ct);
    }

    public Task<Result> ChangeReviewStatusAsync(
        Guid reviewId,
        ModerationStatus status,
        string? rejectReason,
        CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Put)
            .WithEndpoint("api/ModerationReviews")
            .WithBody(new ChangeModerationReviewStatusRequest
            {
                ModerationReviewId = reviewId,
                ModerationStatus = status,
                RejectReason = rejectReason
            })
            .Authorize();

        return Execute(command, ct);
    }
}
