using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateUsername;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events.Account;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Account.Application.Tests.Features.User.UpdateUserProfile.UpdateUsername;

public class UpdateUsernameHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [Fact]
    public async Task Handle_WithValidUserAndUsername_ShouldUpdateUsernameSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "newusername";
        var command = new UpdateProfileUsernameCommand(userId, newUsername);
        
        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ReturnsAsync(1);  // Return 1 to indicate one record was saved

        UserNameChangedEvent capturedEvent = null;
        _eventPublisherMock.Setup(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserNameChangedEvent, CancellationToken>((evt, ct) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        var result = await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(newUsername);
        result.Message.Should().Be("Username updated successful");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
        
        capturedEvent.Should().NotBeNull();
        capturedEvent.NewUserName.Should().Be(newUsername);

        existingUser.Username.Value.Should().Be(newUsername);
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "newusername";
        var command = new UpdateProfileUsernameCommand(userId, newUsername);

        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync((CoffeePeek.Account.Domain.Entities.UserAggregate.User)null);

        // Act
        Func<Task> act = async () => await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidUsername_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invalidUsername = ""; // Invalid username (empty)
        var command = new UpdateProfileUsernameCommand(userId, invalidUsername);

        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Username cannot be empty.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_WithTooShortUsername_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tooShortUsername = "ab"; // Less than minimum length of 3
        var command = new UpdateProfileUsernameCommand(userId, tooShortUsername);

        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Username must be between 3 and 30 characters.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_WithTooLongUsername_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tooLongUsername = new string('a', 31); // More than maximum length of 30
        var command = new UpdateProfileUsernameCommand(userId, tooLongUsername);

        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Username must be between 3 and 30 characters.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidCharactersInUsername_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invalidUsername = "user@invalid"; // Contains invalid character '@'
        var command = new UpdateProfileUsernameCommand(userId, invalidUsername);

        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Username can only contain letters, numbers, dots, and underscores, and must start with a letter.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), _cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_WithSameUsername_ShouldStillUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUsername = "sameusername";
        var command = new UpdateProfileUsernameCommand(userId, existingUsername);

        var existingUser = CreateUserMock(userId, existingUsername);
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ReturnsAsync(1);  // Return 1 to indicate one record was saved

        UserNameChangedEvent capturedEvent = null;
        _eventPublisherMock.Setup(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserNameChangedEvent, CancellationToken>((evt, ct) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        var result = await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(existingUsername);
        result.Message.Should().Be("Username updated successful");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
        
        capturedEvent.Should().NotBeNull();
        capturedEvent.NewUserName.Should().Be(existingUsername);

        existingUser.Username.Value.Should().Be(existingUsername);
    }

    [Fact]
    public async Task Handle_WithDifferentCasingUsername_ShouldUpdateSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "NewUsername";
        var command = new UpdateProfileUsernameCommand(userId, newUsername);

        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ReturnsAsync(1);  // Return 1 to indicate one record was saved

        UserNameChangedEvent capturedEvent = null;
        _eventPublisherMock.Setup(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserNameChangedEvent, CancellationToken>((evt, ct) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        var result = await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(newUsername);
        result.Message.Should().Be("Username updated successful");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
        
        capturedEvent.Should().NotBeNull();
        capturedEvent.NewUserName.Should().Be(newUsername);

        existingUser.Username.Value.Should().Be(newUsername);
    }

    [Fact]
    public async Task Handle_UnitOfWorkSaveChangesThrowsException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "newusername";
        var command = new UpdateProfileUsernameCommand(userId, newUsername);

        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        var exception = new Exception("Database error");
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ThrowsAsync(exception);

        UserNameChangedEvent capturedEvent = null;
        _eventPublisherMock.Setup(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserNameChangedEvent, CancellationToken>((evt, ct) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
        
        capturedEvent.Should().NotBeNull();
        capturedEvent.NewUserName.Should().Be(newUsername);
    }

    [Fact]
    public async Task Handle_EventPublisherThrowsException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newUsername = "newusername";
        var command = new UpdateProfileUsernameCommand(userId, newUsername);

        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        UserNameChangedEvent capturedEvent = null;
        var exception = new Exception("Event publishing error");
        _eventPublisherMock.Setup(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserNameChangedEvent, CancellationToken>((evt, ct) => capturedEvent = evt)
            .ThrowsAsync(exception);

        // Act
        Func<Task> act = async () => await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Event publishing error");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        
        capturedEvent.Should().NotBeNull();
        capturedEvent.NewUserName.Should().Be(newUsername);
    }

    [Theory]
    [InlineData("validuser")]
    [InlineData("user.name")]
    [InlineData("user_name")]
    [InlineData("user.name123")]
    [InlineData("user_name_123")]
    [InlineData("a12345678901234567890123456789")] // Maximum length (30 chars)
    public async Task Handle_WithValidUsernameFormats_ShouldUpdateSuccessfully(string validUsername)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateProfileUsernameCommand(userId, validUsername);

        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ReturnsAsync(1);  // Return 1 to indicate one record was saved

        UserNameChangedEvent capturedEvent = null;
        _eventPublisherMock.Setup(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<UserNameChangedEvent, CancellationToken>((evt, ct) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        var result = await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(validUsername);
        result.Message.Should().Be("Username updated successful");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
        
        capturedEvent.Should().NotBeNull();
        capturedEvent.NewUserName.Should().Be(validUsername);

        existingUser.Username.Value.Should().Be(validUsername);
    }

    [Theory]
    [InlineData("123user")] // Starts with number
    [InlineData(".username")] // Starts with dot
    [InlineData("_username")] // Starts with underscore
    [InlineData("user@name")] // Contains @
    [InlineData("user name")] // Contains space
    [InlineData("user-name")] // Contains dash
    public async Task Handle_WithInvalidUsernameFormats_ShouldThrowDomainException(string invalidUsername)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateProfileUsernameCommand(userId, invalidUsername);

        var existingUser = CreateUserMock(userId, "oldusername");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUsernameHandler.Handle(command, _userRepositoryMock.Object, 
            _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>();

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<UserNameChangedEvent>(), _cancellationToken), Times.Never);
    }

    private CoffeePeek.Account.Domain.Entities.UserAggregate.User CreateUserMock(Guid userId, string username)
    {
        // Since this is in the namespace CoffeePeek.Account.Application.Tests.Features.User..., 
        // we need to use the fully qualified name to avoid confusion with the namespace
        var user = CoffeePeek.Account.Domain.Entities.UserAggregate.User.CreateExternal("test@example.com", "external_provider", "provider_id");
        user.UpdateUsername(Username.Create(username));
        
        // Set the Id using reflection
        var aggregateRootType = typeof(CoffeePeek.Shared.Domain.Entities.AggregateRoot<Guid>);
        var idProperty = aggregateRootType.GetProperty("Id", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idProperty?.SetValue(user, userId);
        
        return user;
    }
}