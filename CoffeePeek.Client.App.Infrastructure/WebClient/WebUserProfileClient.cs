using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebUserProfileClient(IHttpCommandExecutor httpCommandExecutor) : IWebUserProfileClient
{
    public async Task<Result<UserProfileDto>> GetPublicProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Get)
            .WithEndpoint($"api/Users/{userId:D}");

        var api = await httpCommandExecutor.Execute<UserProfileDto>(command, cancellationToken);
        if (!api.IsSuccess || api.Data is null)
            return Result.Fail(api.Message ?? "Failed to load profile.");

        return Result.Ok(api.Data);
    }
}
