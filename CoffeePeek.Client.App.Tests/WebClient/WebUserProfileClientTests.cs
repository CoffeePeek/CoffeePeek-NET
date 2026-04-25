using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Abstract;
using CoffeePeek.Client.App.Infrastructure.HTTP.Pipeline.Models;
using CoffeePeek.Client.App.Infrastructure.HTTP.Responses;
using CoffeePeek.Client.App.Infrastructure.WebClient;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Client.App.Tests.WebClient;

public class WebUserProfileClientTests
{
    private readonly Mock<IHttpCommandExecutor> _executorMock = new();

    private WebUserProfileClient CreateSut() => new(_executorMock.Object);

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
}
