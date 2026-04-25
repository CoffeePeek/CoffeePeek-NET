using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Profile;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Contract.Dtos;
using FluentResults;
using System.Net.Http.Headers;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebUserProfileClient(IHttpCommandExecutor httpCommandExecutor, HttpClient httpClient)
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

    public async Task<Result> UploadAvatarAsync(
        string fileName,
        string contentType,
        byte[] fileContent,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileContent.Length == 0)
            return Result.Fail("Avatar file is invalid.");

        var safeContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType;

        var prepareCommand = new HttpCommand()
            .WithMethod(HttpMethod.Post)
            .WithEndpoint("api/Photos/avatar")
            .WithBody(new GenerateAvatarUploadUrlRequest
            {
                SizeBytes = fileContent.Length,
                FileName = fileName,
                ContentType = safeContentType
            })
            .Authorize();

        var prepareResult = await Execute<GenerateUploadUrlResponseDto>(prepareCommand, ct);
        if (prepareResult.IsFailed)
            return Result.Fail(prepareResult.Errors);

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, prepareResult.Value.UploadUrl)
        {
            Content = new ByteArrayContent(fileContent)
        };
        putRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(safeContentType);

        using var putResponse = await httpClient.SendAsync(putRequest, ct);
        if (!putResponse.IsSuccessStatusCode)
            return Result.Fail($"Avatar upload failed with status {(int)putResponse.StatusCode}.");

        var finalizeCommand = new HttpCommand()
            .WithMethod(HttpMethod.Patch)
            .WithEndpoint("api/Users/me/avatar")
            .WithBody(new UpdateAvatarRequest
            {
                UploadedPhoto = new UploadedPhotoDto(
                    fileName,
                    safeContentType,
                    prepareResult.Value.StorageKey,
                    fileContent.LongLength)
            })
            .Authorize();

        return await Execute(finalizeCommand, ct);
    }
}
