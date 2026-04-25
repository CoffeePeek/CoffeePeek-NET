using System.Net;
using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebUsersClient(IHttpCommandExecutor httpCommandExecutor) : IWebUsersClient
{
    public async Task<Result<bool>> EmailIsRegisteredAsync(string email, CancellationToken cancellationToken = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Get)
            .WithEndpoint("api/Users/exists")
            .WithQuery("email", email);

        var api = await httpCommandExecutor.Execute<bool>(command, cancellationToken);
        const int notFound = (int)HttpStatusCode.NotFound;

        if (api.StatusCode == notFound)
            return Result.Ok(false);

        if (api is { IsSuccess: true, Data: true })
            return Result.Ok(true);

        if (api is { IsSuccess: true, Data: false })
            return Result.Ok(false);

        return Result.Fail(api.Message ?? "Unable to verify email.");
    }
}
