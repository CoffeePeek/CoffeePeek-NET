using CoffeePeek.Client.App.Infrastructure.HTTP.Extensions;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Profile;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.HTTP.WebClients;
using CoffeePeek.Contract.Dtos;
using FluentResults;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace CoffeePeek.Client.App.Infrastructure.WebClient;

public sealed class WebUserProfileClient(
    IHttpCommandExecutor httpCommandExecutor,
    HttpClient httpClient,
    ILogger<WebUserProfileClient> logger)
    : WebClientBase(httpCommandExecutor), IWebUserProfileClient
{
    private const int FinalizeAvatarMaxAttempts = 3;

    private const long MaxAvatarBytes = 10L * 1024 * 1024;

    private static bool IsTransientFinalizeFailure(Result result)
    {
        if (result is null || !result.IsFailed)
            return false;

        var text = string.Join(" ", result.Errors.Select(e => e.Message));
        if (text.Contains("canceled", StringComparison.OrdinalIgnoreCase))
            return false;
        if (Regex.IsMatch(text, @"\b(408|429)\b", RegexOptions.None, TimeSpan.FromMilliseconds(200)))
            return true;
        if (Regex.IsMatch(text, @"\b5\d\d\b", RegexOptions.None, TimeSpan.FromMilliseconds(200)))
            return true;
        if (text.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            return true;
        if (text.Contains("temporarily", StringComparison.OrdinalIgnoreCase))
            return true;
        if (text.Contains("connection", StringComparison.OrdinalIgnoreCase) &&
            (text.Contains("refused", StringComparison.OrdinalIgnoreCase) ||
             text.Contains("reset", StringComparison.OrdinalIgnoreCase)))
            return true;
        return false;
    }

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
        if (string.IsNullOrWhiteSpace(fileName) || fileContent is null || fileContent.Length == 0)
            return Result.Fail("Avatar file is invalid.");

        if (fileContent.LongLength > MaxAvatarBytes)
            return Result.Fail("Avatar is too large (maximum 10 MB).");

        var safeContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType;

        if (!MediaTypeHeaderValue.TryParse(safeContentType, out var mediaType))
            return Result.Fail($"Invalid content type: {safeContentType}");

        var prepareCommand = new HttpCommand()
            .WithMethod(HttpMethod.Post)
            .WithEndpoint("api/Photos/avatar")
            .WithBody(new GenerateAvatarUploadUrlRequest
            {
                SizeBytes = fileContent.LongLength,
                FileName = fileName,
                ContentType = safeContentType
            })
            .Authorize();

        var prepareResult = await Execute<GenerateUploadUrlResponseDto>(prepareCommand, ct);
        if (prepareResult.IsFailed)
            return prepareResult.ToResult();

        using var putRequest = new HttpRequestMessage(HttpMethod.Put, prepareResult.Value.UploadUrl)
        {
            Content = new ByteArrayContent(fileContent)
        };
        putRequest.Content.Headers.ContentType = mediaType;

        using var putResponse = await httpClient.SendAsync(putRequest, ct);
        if (!putResponse.IsSuccessStatusCode)
            return Result.Fail($"Avatar upload failed with status {(int)putResponse.StatusCode}.");

        var finalizeCommand = new HttpCommand()
            .WithMethod(HttpMethod.Patch)
            .WithEndpoint("api/Users/me/avatar")
            .WithBody(new UpdateAvatarRequest
            {
                UploadedPhoto = new UploadedPhotoDto(
                    FileName: fileName,
                    ContentType: safeContentType,
                    StorageKey: prepareResult.Value.StorageKey,
                    Size: fileContent.LongLength)
            })
            .Authorize();

        Result? lastError = null;
        for (var attempt = 1; attempt <= FinalizeAvatarMaxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            var finalizeResult = await Execute(finalizeCommand, ct);
            if (finalizeResult.IsSuccess)
                return finalizeResult;

            lastError = finalizeResult;
            if (attempt == FinalizeAvatarMaxAttempts)
                break;
            if (!IsTransientFinalizeFailure(finalizeResult))
                break;

            var baseDelayMs = 200 * (1 << (attempt - 1));
            var jitterMs = Random.Shared.Next(0, 120);
            await Task.Delay(baseDelayMs + jitterMs, ct);
        }

        logger.LogError(
            "Avatar finalize failed after retries. StorageKey={StorageKey}, FileName={FileName}, ContentType={ContentType}, Error={Error}",
            prepareResult.Value.StorageKey,
            fileName,
            safeContentType,
            lastError?.Errors.FirstOrDefault()?.Message ?? "unknown");

        return lastError ?? Result.Fail("Avatar finalize failed.");
    }
}
