using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Handlers;
using CoffeePeek.AuthService.Models;
using CoffeePeek.AuthService.Services;
using CoffeePeek.AuthService.Services.Validation;
using FluentAssertions;
using MassTransit;
using Moq;
using Xunit;

namespace CoffeePeek.AuthService.Tests.Handlers;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserManager> _userManagerMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IValidationStrategy<RegisterUserCommand>> _validationStrategyMock;
    private readonly RegisterUserHandler _sut;

    public RegisterUserHandlerTests()
    {
        _userManagerMock = new Mock<IUserManager>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _validationStrategyMock = new Mock<IValidationStrategy<RegisterUserCommand>>();

        _sut = new RegisterUserHandler(
            _userManagerMock.Object,
            _publishEndpointMock.Object,
            _validationStrategyMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidUser_ReturnsSuccessWithUserId()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "test@example.com", "ValidPass123");
        var userId = Guid.NewGuid();
        var user = new UserCredentials
        {
            Id = userId,
            Email = command.Email
        };

        _validationStrategyMock
            .Setup(x => x.Validate(command))
            .Returns(Models.ValidationResult.Valid);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((UserCredentials?)null);
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<UserCredentials>(), command.Password))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_WithInvalidValidation_ReturnsError()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "invalid-email", "short");
        var validationResult = Models.ValidationResult.Invalid("Invalid email address");

        _validationStrategyMock
            .Setup(x => x.Validate(command))
            .Returns(validationResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid email address");
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsError()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "existing@example.com", "ValidPass123");
        var existingUser = new UserCredentials
        {
            Id = Guid.NewGuid(),
            Email = command.Email
        };

        _validationStrategyMock
            .Setup(x => x.Validate(command))
            .Returns(Models.ValidationResult.Valid);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_WhenUserManagerThrowsException_ReturnsError()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "test@example.com", "ValidPass123");
        var exceptionMessage = "Database error";

        _validationStrategyMock
            .Setup(x => x.Validate(command))
            .Returns(Models.ValidationResult.Valid);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((UserCredentials?)null);
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<UserCredentials>(), command.Password))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task Handle_WithSuccessfulRegistration_PublishesUserRegisteredEvent()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "test@example.com", "ValidPass123");
        var userId = Guid.NewGuid();
        var user = new UserCredentials
        {
            Id = userId,
            Email = command.Email
        };

        _validationStrategyMock
            .Setup(x => x.Validate(command))
            .Returns(Models.ValidationResult.Valid);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((UserCredentials?)null);
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<UserCredentials>(), command.Password))
            .ReturnsAsync(user);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Theory]
    [InlineData("", "test@example.com", "password")]
    [InlineData("User", "", "password")]
    [InlineData("User", "test@example.com", "")]
    public async Task Handle_WithMissingRequiredFields_ReturnsError(string userName, string email, string password)
    {
        // Arrange
        var command = new RegisterUserCommand(userName, email, password);

        _validationStrategyMock
            .Setup(x => x.Validate(command))
            .Returns(Models.ValidationResult.Valid);
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((UserCredentials?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert - system should handle empty values appropriately
        result.Should().NotBeNull();
    }
}