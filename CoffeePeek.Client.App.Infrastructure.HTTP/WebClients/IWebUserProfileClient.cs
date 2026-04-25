using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public interface IWebUserProfileClient
{
    Task<Result<UserProfileDto>> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
