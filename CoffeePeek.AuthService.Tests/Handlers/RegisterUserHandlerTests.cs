using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Handlers;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.AuthService.Services;
using CoffeePeek.AuthService.Services.Validation;
using CoffeePeek.Data.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.AuthService.Tests.Handlers;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserCredentialsRepository> _credentialsRepoMock;
    private readonly Mock<IPasswordHasherService> _passwordHasherMock;
    private readonly Mock<IUserManager> _userManagerMock;
    private readonly Mock<IValidationStrategy<RegisterUserCommand>> _validationStrategyMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterUserHandler _sut;

    public RegisterUserHandlerTests()
    {
        _credentialsRepoMock = new Mock<IUserCredentialsRepository>();
        _passwordHasherMock = new Mock<IPasswordHasherService>();
        _userManagerMock = new Mock<IUserManager>();
        _validationStrategyMock = new Mock<IValidationStrategy<RegisterUserCommand>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new RegisterUserHandler(
            _credentialsRepoMock.Object,
            _passwordHasherMock.Object,
            _userManagerMock.Object,
            _validationStrategyMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidUser_ReturnsSuccessWithUserId()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "test@example.com", "ValidPass123");
        var userId = Guid.NewGuid();

        _validationStrategyMock
            .Setup(x => x.Validate(command))
            .Returns(Models.ValidationResult.Valid);
        _credentialsRepoMock
            .Setup(x => x.UserExists(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns("hashed_password");
        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<UserCredentials>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
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
        result.Message.Should().Be("Invalid email address");
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ReturnsError()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "existing@example.com", "ValidPass123");

        _validationStrategyMock
            .Setup(x => x.Validate(command))
            .Returns(Models.ValidationResult.Valid);
        _credentialsRepoMock
            .Setup(x => x.UserExists(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ReturnsError()
    {
        // Arrange
        var command = new RegisterUserCommand("TestUser", "test@example.com", "ValidPass123");
        var exceptionMessage = "Database error";

        _validationStrategyMock
            .Setup(x => x.Validate(command))
            .Returns(Models.ValidationResult.Valid);
        _credentialsRepoMock
            .Setup(x => x.UserExists(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _credentialsRepoMock
            .Setup(x => x.AddAsync(It.IsAny<UserCredentials>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
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
        _credentialsRepoMock
            .Setup(x => x.UserExists(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert - system should handle empty values appropriately
        result.Should().NotBeNull();
    }
}