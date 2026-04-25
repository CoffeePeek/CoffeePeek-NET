using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Profile;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebUserProfileClient(IHttpCommandExecutor httpCommandExecutor)
    : WebClientBase(httpCommandExecutor), IWebUserProfileClient
{
    public Task<Result<UserProfileDto>> GetPublicProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new HttpCommand()
            .WithEndpoint($"api/Users/{userId:D}");

        return Execute<UserProfileDto>(command, cancellationToken);
    }

    public Task<Result> UpdateAboutAsync(string about, CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Patch)
            .WithEndpoint("api/Users/me/about")
            .WithBody(new UpdateAboutRequest { About = about })
            .Authorize();

        return Execute(command, ct);
    }

    public Task<Result> UpdateUsernameAsync(string username, CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Patch)
            .WithEndpoint("api/Users/me/username")
            .WithBody(new UpdateUsernameRequest { Username = username })
            .Authorize();

        return Execute(command, ct);
    }

    public Task<Result> UpdatePhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
    {
        var command = new HttpCommand()
            .WithMethod(HttpMethod.Patch)
            .WithEndpoint("api/Users/me/phone-number")
            .WithBody(new UpdatePhoneNumberRequest { PhoneNumber = phoneNumber })
            .Authorize();

        return Execute(command, ct);
    }
}
