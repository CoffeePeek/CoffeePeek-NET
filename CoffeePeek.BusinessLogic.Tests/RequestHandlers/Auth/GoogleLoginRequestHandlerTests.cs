using CoffeePeek.BusinessLogic.RequestHandlers;
using CoffeePeek.Contract.Dtos.Auth;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Infrastructure.Auth;
using CoffeePeek.Infrastructure.Services;
using Google.Apis.Auth;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.Auth;

public class GoogleLoginRequestHandlerTests
{
    private readonly Mock<IGoogleAuthService> _googleAuthServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IJWTTokenService> _jwtTokenServiceMock;
    
    private readonly GoogleLoginRequestHandler _handler;
    
    public GoogleLoginRequestHandlerTests()
    {
        _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        _userServiceMock = new Mock<IUserService>();
        _jwtTokenServiceMock = new Mock<IJWTTokenService>();

        _handler = new GoogleLoginRequestHandler(
            _googleAuthServiceMock.Object,
            _userServiceMock.Object,
            _jwtTokenServiceMock.Object
        );
    }
    
    [Fact]
    public async Task Handle_ShouldReturnError_WhenIdTokenIsEmpty()
    {
        // Arrange
        var request = new GoogleLoginRequest { IdToken = "" };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("IdToken is required", result.Message);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenGoogleTokenIsInvalid()
    {
        // Arrange
        var request = new GoogleLoginRequest { IdToken = "invalid-token" };

        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((GoogleJsonWebSignature.Payload?)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid Google token", result.Message);
    }

    [Fact]
    public async Task Handle_ShouldCallDomainServiceAndReturnJwt_WhenTokenIsValid()
    {
        // Arrange
        var request = new GoogleLoginRequest { IdToken = "valid-token" };

        var googleUser = new GoogleJsonWebSignature.Payload
        {
            Email = "test@example.com",
            Name = "John Doe",
            Picture =  "http://pic.png",
            Subject = "123456"
        };

        var domainUser = new Domain.Entities.Users.User
        {
            UserName = "John Doe",
            Email = "test@example.com",
            GoogleId = "123456",
            AvatarUrl = "http://pic.png"
        };

        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync("valid-token"))
            .ReturnsAsync(googleUser);

        _userServiceMock
            .Setup(s => s.GetOrCreateGoogleUserAsync(
                googleUser.Subject,
                googleUser.Email,
                googleUser.Name,
                googleUser.Picture))
            .ReturnsAsync(domainUser);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateTokensAsync(domainUser).Result)
            .Returns(new AuthResult {AccessToken = "jwt-token"});

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("jwt-token", result.Data.AccessToken);

        Assert.Equal(domainUser.Email, result.Data.User.Email);
        Assert.Equal(domainUser.UserName, result.Data.User.Username);
        Assert.Equal(domainUser.AvatarUrl, result.Data.User.AvatarUrl);

        _googleAuthServiceMock.Verify(s => s.ValidateIdTokenAsync("valid-token"), Times.Once);
        _userServiceMock.Verify(s =>
            s.GetOrCreateGoogleUserAsync(
                googleUser.Subject,
                googleUser.Email,
                googleUser.Name,
                googleUser.Picture),
            Times.Once);

        _jwtTokenServiceMock.Verify(j => j.GenerateTokensAsync(domainUser), Times.Once);
    }
}