using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebUserReviewsClient(IHttpCommandExecutor httpCommandExecutor) : IWebUserReviewsClient
{
    public async Task<Result<GetReviewsByUserIdResultDto>> GetReviewsAsync(
        Guid userId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Get)
            .WithEndpoint($"api/users/{userId:D}/reviews")
            .WithQuery("pageNumber", pageNumber.ToString())
            .WithQuery("pageSize", pageSize.ToString())
            .Authorize();

        var api = await httpCommandExecutor.Execute<GetReviewsByUserIdResultDto>(command, cancellationToken);
        if (!api.IsSuccess || api.Data is null)
            return Result.Fail(api.Message ?? "Failed to load reviews.");

        return Result.Ok(api.Data);
    }
}
