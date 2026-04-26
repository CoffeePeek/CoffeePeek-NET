using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.WebClient;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Net;

namespace CoffeePeek.Client.App.Tests.WebClient;

public class WebUserProfileClientTests
{
    private readonly Mock<IHttpCommandExecutor> _executorMock = new();
    private readonly Mock<ILogger<WebUserProfileClient>> _loggerMock = new();
    private readonly FakeHttpMessageHandler _httpMessageHandler = new();

    private WebUserProfileClient CreateSut()
    {
        var httpClient = new HttpClient(_httpMessageHandler);
        return new WebUserProfileClient(_executorMock.Object, httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task UpdateAboutAsync_SendsPatchWithCorrectEndpoint()
    {
        HttpCommand? captured = null;
        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((cmd, _) => captured = cmd)
            .ReturnsAsync(new ApiResponse { IsSuccess = true, StatusCode = 202 });

        var sut = CreateSut();
        var result = await sut.UpdateAboutAsync("New bio");

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be("api/Users/me/about");
        captured.Method.Should().Be(HttpMethod.Patch);
        captured.IsAuthorize.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUsernameAsync_SendsPatchWithCorrectEndpoint()
    {
        HttpCommand? captured = null;
        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((cmd, _) => captured = cmd)
            .ReturnsAsync(new ApiResponse { IsSuccess = true, StatusCode = 202 });

        var sut = CreateSut();
        var result = await sut.UpdateUsernameAsync("newuser");

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be("api/Users/me/username");
        captured.Method.Should().Be(HttpMethod.Patch);
        captured.IsAuthorize.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePhoneNumberAsync_SendsPatchWithCorrectEndpoint()
    {
        HttpCommand? captured = null;
        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((cmd, _) => captured = cmd)
            .ReturnsAsync(new ApiResponse { IsSuccess = true, StatusCode = 202 });

        var sut = CreateSut();
        var result = await sut.UpdatePhoneNumberAsync("+79001234567");

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Endpoint.Should().Be("api/Users/me/phone-number");
        captured.Method.Should().Be(HttpMethod.Patch);
        captured.IsAuthorize.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAboutAsync_ReturnsFailure_WhenApiError()
    {
        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse { IsSuccess = false, StatusCode = 400, Message = "Bad request" });

        var sut = CreateSut();
        var result = await sut.UpdateAboutAsync("x");

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Bad request");
    }

    [Fact]
    public async Task GetPublicProfileAsync_ReturnsProfile()
    {
        var userId = Guid.NewGuid();
        _executorMock
            .Setup(e => e.Execute<UserProfileDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<UserProfileDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Data = new UserProfileDto { UserName = "test", Email = "t@t.com" }
            });

        var sut = CreateSut();
        var result = await sut.GetPublicProfileAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserName.Should().Be("test");
    }

    [Fact]
    public async Task UploadAvatarAsync_PerformsPutAndFinalizePatch()
    {
        var callIndex = 0;
        HttpCommand? prepareCommand = null;
        HttpCommand? finalizeCommand = null;

        _executorMock
            .Setup(e => e.Execute<GenerateUploadUrlResponseDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((cmd, _) => prepareCommand = cmd)
            .ReturnsAsync(new ApiResponse<GenerateUploadUrlResponseDto>
            {
                IsSuccess = true,
                Data = new GenerateUploadUrlResponseDto
                {
                    UploadUrl = "https://storage.test/upload",
                    StorageKey = "avatars/test.jpg"
                }
            });

        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .Callback<HttpCommand, CancellationToken>((cmd, _) =>
            {
                callIndex++;
                finalizeCommand = cmd;
            })
            .ReturnsAsync(new ApiResponse { IsSuccess = true, StatusCode = (int)HttpStatusCode.Accepted });

        _httpMessageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK);

        var sut = CreateSut();
        var result = await sut.UploadAvatarAsync("avatar.jpg", "image/jpeg", [1, 2, 3]);

        result.IsSuccess.Should().BeTrue();
        prepareCommand.Should().NotBeNull();
        prepareCommand!.Endpoint.Should().Be("api/Photos/avatar");
        prepareCommand.Method.Should().Be(HttpMethod.Post);
        prepareCommand.IsAuthorize.Should().BeTrue();

        _httpMessageHandler.LastRequest.Should().NotBeNull();
        _httpMessageHandler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        _httpMessageHandler.LastRequest.RequestUri!.ToString().Should().Be("https://storage.test/upload");
        _httpMessageHandler.LastRequest.Content!.Headers.ContentType!.MediaType.Should().Be("image/jpeg");

        callIndex.Should().Be(1);
        finalizeCommand.Should().NotBeNull();
        finalizeCommand!.Endpoint.Should().Be("api/Users/me/avatar");
        finalizeCommand.Method.Should().Be(HttpMethod.Patch);
        finalizeCommand.IsAuthorize.Should().BeTrue();
    }

    [Fact]
    public async Task UploadAvatarAsync_ReturnsFailure_WhenPutFails()
    {
        _executorMock
            .Setup(e => e.Execute<GenerateUploadUrlResponseDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GenerateUploadUrlResponseDto>
            {
                IsSuccess = true,
                Data = new GenerateUploadUrlResponseDto
                {
                    UploadUrl = "https://storage.test/upload",
                    StorageKey = "avatars/test.jpg"
                }
            });

        _executorMock
            .Setup(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse { IsSuccess = true, StatusCode = (int)HttpStatusCode.Accepted });

        _httpMessageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.BadRequest);

        var sut = CreateSut();
        var result = await sut.UploadAvatarAsync("avatar.jpg", "image/jpeg", [1, 2, 3]);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Avatar upload failed"));
        _executorMock.Verify(e => e.Execute(It.Is<HttpCommand>(c => c.Endpoint == "api/Users/me/avatar"), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadAvatarAsync_ReturnsFailure_WhenContentTypeInvalid()
    {
        var sut = CreateSut();
        var result = await sut.UploadAvatarAsync("a.jpg", "not a real media type", [1, 2, 3]);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Invalid content type", StringComparison.Ordinal));
        _executorMock.Verify(
            e => e.Execute<GenerateUploadUrlResponseDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _httpMessageHandler.LastRequest.Should().BeNull();
    }

    [Fact]
    public async Task UploadAvatarAsync_ReturnsFailure_WhenFileNameIsEmpty()
    {
        var sut = CreateSut();
        var result = await sut.UploadAvatarAsync(" ", "image/jpeg", [1, 2, 3]);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Avatar file is invalid.");
        _executorMock.Verify(e => e.Execute<GenerateUploadUrlResponseDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _executorMock.Verify(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _httpMessageHandler.LastRequest.Should().BeNull();
    }

    [Fact]
    public async Task UploadAvatarAsync_ReturnsFailure_WhenFileContentEmpty()
    {
        var sut = CreateSut();
        var result = await sut.UploadAvatarAsync("avatar.jpg", "image/jpeg", []);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Avatar file is invalid.");
        _executorMock.Verify(e => e.Execute<GenerateUploadUrlResponseDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _executorMock.Verify(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _httpMessageHandler.LastRequest.Should().BeNull();
    }

    [Fact]
    public async Task UploadAvatarAsync_ReturnsFailure_WhenFileContentNull()
    {
        var sut = CreateSut();
        var result = await sut.UploadAvatarAsync("avatar.jpg", "image/jpeg", null!);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "Avatar file is invalid.");
        _executorMock.Verify(e => e.Execute<GenerateUploadUrlResponseDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _executorMock.Verify(e => e.Execute(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _httpMessageHandler.LastRequest.Should().BeNull();
    }

    [Fact]
    public async Task UploadAvatarAsync_ReturnsFailure_WhenPrepareStepFails()
    {
        _executorMock
            .Setup(e => e.Execute<GenerateUploadUrlResponseDto>(It.IsAny<HttpCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse<GenerateUploadUrlResponseDto>
            {
                IsSuccess = false,
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "prepare failed"
            });

        var sut = CreateSut();
        var result = await sut.UploadAvatarAsync("avatar.jpg", "image/jpeg", [1, 2, 3]);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == "prepare failed");
        _httpMessageHandler.LastRequest.Should().BeNull();
        _executorMock.Verify(e => e.Execute(It.Is<HttpCommand>(c => c.Endpoint == "api/Users/me/avatar"), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = ResponseFactory?.Invoke(request) ?? new HttpResponseMessage(HttpStatusCode.OK);
            return Task.FromResult(response);
        }
    }
}
