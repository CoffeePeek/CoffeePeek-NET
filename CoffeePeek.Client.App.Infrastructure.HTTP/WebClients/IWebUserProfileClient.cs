using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using FluentResults;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;

public interface IWebUserProfileClient
{
    Task<Result<UserProfileDto>> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result> UpdateAboutAsync(string about, CancellationToken ct = default);

    Task<Result> UpdateUsernameAsync(string username, CancellationToken ct = default);

    Task<Result> UpdatePhoneNumberAsync(string phoneNumber, CancellationToken ct = default);

    Task<Result> UploadAvatarAsync(
        string fileName,
        string contentType,
        Stream fileContent,
        long contentLength,
        CancellationToken ct = default);
}
