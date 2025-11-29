using CoffeePeek.Api.Controllers;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Contract.Response.Login;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Moq;

namespace CoffeePeek.Api.Test.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController(_mediatorMock.Object);
    }

    [Fact]
    public async Task CheckUserExistsByEmail_ShouldReturnResponse_WhenEmailProvided()
    {
        // Arrange
        var email = "test@example.com";
        var expectedResponse = new Response { Success = true, Message = "User exists" };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CheckUserExistsByEmailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CheckUserExistsByEmail(email);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("User exists");

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<CheckUserExistsByEmailRequest>(r => r.Email == email),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_ShouldReturnLoginResponse_WhenValidCredentials()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "password123");
        var expectedResponse = new Response<LoginResponse>
        {
            Success = true,
            Data = new LoginResponse("access-token", "refresh-token")
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access-token");
        result.Data.RefreshToken.Should().Be("refresh-token");

        _mediatorMock.Verify(
            m => m.Send(loginRequest, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_ShouldReturnRegisterUserResponse_WhenValidData()
    {
        // Arrange
        var registerRequest = new RegisterUserRequest("test@example.com", "password123", "Test User");
        var expectedResponse = new Response<RegisterUserResponse>
        {
            Success = true,
            Data = new RegisterUserResponse
            {
                Email = "test@example.com",
                UserName = "Test User"
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RegisterUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be("test@example.com");
        result.Data.UserName.Should().Be("Test User");

        _mediatorMock.Verify(
            m => m.Send(registerRequest, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_ShouldReturnFailedResponse_WhenInvalidCredentials()
    {
        // Arrange
        var loginRequest = new LoginRequest("test@example.com", "wrong-password");
        var expectedResponse = new Response<LoginResponse>
        {
            Success = false,
            Message = "Invalid credentials"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNewTokens_WhenValidRefreshToken()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var userId = 1;
        var expectedResponse = new Response<GetRefreshTokenResponse>
        {
            Success = true,
            Data = new GetRefreshTokenResponse("new-access-token", "new-refresh-token")
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetRefreshTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Setup HttpContext with authenticated user
        var httpContext = CreateHttpContextWithAuthenticatedUser(userId);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _controller.RefreshToken(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("new-access-token");
        result.Data.RefreshToken.Should().Be("new-refresh-token");

        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetRefreshTokenRequest>(r => r.RefreshToken == refreshToken && r.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshToken_ShouldThrowUnauthorizedException_WhenUserNotAuthenticated()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await _controller.Invoking(c => c.RefreshToken(refreshToken))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User ID claim is missing.");

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetRefreshTokenRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static HttpContext CreateHttpContextWithAuthenticatedUser(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"testuser{userId}")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        return httpContext;
    }
}