using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAvatar;
using CoffeePeek.Account.Domain.Entities.PhotoMetadataAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Events;

using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using FluentAssertions;
using Moq;
using Xunit;

namespace CoffeePeek.Account.Application.Tests.Features.User.UpdateUserProfile.UpdateAvatar;

public class UpdateUserAvatarRequestHandlerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPhotoMetadataRepository> _photoMetadataRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [Fact]
    public async Task Handle_WithValidUserAndPhoto_ShouldUpdateAvatarSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("avatar.jpg", "image/jpeg", "avatars/user123.jpg", 102400);
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);
        
        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ReturnsAsync(1);  // Return 1 to indicate one record was saved

        PhotoReplacedEvent capturedEvent = null;
        _eventPublisherMock.Setup(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<PhotoReplacedEvent, CancellationToken>((evt, ct) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        var result = await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Message.Should().Be("Photo updated successfully");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);

        existingUser.PhotoMetadata.Should().NotBeNull();
        existingUser.PhotoMetadata!.FileName.Should().Be("avatar.jpg");
        existingUser.PhotoMetadata.ContentType.Should().Be("image/jpeg");
        existingUser.PhotoMetadata.StorageKey.Should().Be("avatars/user123.jpg");
        existingUser.PhotoMetadata.SizeBytes.Should().Be(102400);
    }

    [Fact]
    public async Task Handle_WithValidUserAndPhoto_WhenUserHasOldPhoto_ShouldPublishPhotoReplacedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldPhotoId = Guid.NewGuid();
        var newPhotoId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("new_avatar.jpg", "image/jpeg", "avatars/user456.jpg", 204800);
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);
        
        var existingUser = CreateUserMock(userId, "testuser");
        var oldPhoto = CreatePhotoMetadata(oldPhotoId, "old_avatar.jpg", "image/png", "avatars/old.jpg", 153600);
        existingUser.UpdateAvatar(oldPhoto); // Set initial avatar
        
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        _photoMetadataRepositoryMock.Setup(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()))
            .Callback<CoffeePeek.Account.Domain.Entities.PhotoMetadata>(photo => 
            {
                // Set the new photo's ID to our predefined newPhotoId for predictable testing
                var field = typeof(CoffeePeek.Account.Domain.Entities.PhotoMetadata).GetField("Id", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(photo, newPhotoId);
            });

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ReturnsAsync(1);  // Return 1 to indicate one record was saved

        PhotoReplacedEvent capturedEvent = null;
        _eventPublisherMock.Setup(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<PhotoReplacedEvent, CancellationToken>((evt, ct) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        var result = await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Message.Should().Be("Photo updated successfully");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Once);

        capturedEvent.Should().NotBeNull();
        capturedEvent.OldStorageKey.Should().Be("avatars/old.jpg");
        capturedEvent.OwnerType.Should().Be("User");
        capturedEvent.OwnerId.Should().NotBeEmpty(); // Just verify it's a valid GUID from the user
        capturedEvent.OldPhotoId.Should().NotBeEmpty(); // Just verify it's a valid GUID
        capturedEvent.NewPhotoId.Should().NotBeEmpty(); // Just verify it's a valid GUID

        existingUser.PhotoMetadata.Should().NotBeNull();
        existingUser.PhotoMetadata!.FileName.Should().Be("new_avatar.jpg");
        existingUser.PhotoMetadata.ContentType.Should().Be("image/jpeg");
        existingUser.PhotoMetadata.StorageKey.Should().Be("avatars/user456.jpg");
        existingUser.PhotoMetadata.SizeBytes.Should().Be(204800);
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("avatar.jpg", "image/jpeg", "avatars/user123.jpg", 102400);
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync((CoffeePeek.Account.Domain.Entities.UserAggregate.User)null);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID {userId} not found");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyFileName_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("", "image/jpeg", "avatars/user123.jpg", 102400); // Empty filename
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("File name cannot be empty.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyStorageKey_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("avatar.jpg", "image/jpeg", "", 102400); // Empty storage key
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Storage key is required.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithZeroSize_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("avatar.jpg", "image/jpeg", "avatars/user123.jpg", 0); // Zero size
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("File size must be positive.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeSize_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("avatar.jpg", "image/jpeg", "avatars/user123.jpg", -1024); // Negative size
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("File size must be positive.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnitOfWorkSaveChangesThrowsException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("avatar.jpg", "image/jpeg", "avatars/user123.jpg", 102400);
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        var exception = new Exception("Database error");
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ThrowsAsync(exception);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_PhotoMetadataRepositoryAddThrowsException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("avatar.jpg", "image/jpeg", "avatars/user123.jpg", 102400);
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        var exception = new Exception("Repository error");
        _photoMetadataRepositoryMock.Setup(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()))
            .Throws(exception);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Repository error");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_EventPublisherThrowsException_WhenReplacingPhoto_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldPhotoId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("new_avatar.jpg", "image/jpeg", "avatars/user456.jpg", 204800);
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);
        
        var existingUser = CreateUserMock(userId, "testuser");
        var oldPhoto = CreatePhotoMetadata(oldPhotoId, "old_avatar.jpg", "image/png", "avatars/old.jpg", 153600);
        existingUser.UpdateAvatar(oldPhoto); // Set initial avatar
        
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ReturnsAsync(1);  // Return 1 to indicate one record was saved

        var exception = new Exception("Event publishing error");
        _eventPublisherMock.Setup(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Event publishing error");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never); // Should not reach here due to exception
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("image/png", "avatar.png", "avatars/user123.png", 51200)]
    [InlineData("image/gif", "avatar.gif", "avatars/user123.gif", 256000)]
    [InlineData("image/webp", "avatar.webp", "avatars/user123.webp", 76800)]
    [InlineData("image/bmp", "avatar.bmp", "avatars/user123.bmp", 1024000)]
    public async Task Handle_WithValidImageTypes_ShouldUpdateSuccessfully(string contentType, string fileName, string storageKey, long fileSize)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto(fileName, contentType, storageKey, fileSize);
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(_cancellationToken))
            .ReturnsAsync(1);  // Return 1 to indicate one record was saved

        // Act
        var result = await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Message.Should().Be("Photo updated successfully");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Once);

        existingUser.PhotoMetadata.Should().NotBeNull();
        existingUser.PhotoMetadata!.FileName.Should().Be(fileName);
        existingUser.PhotoMetadata.ContentType.Should().Be(contentType);
        existingUser.PhotoMetadata.StorageKey.Should().Be(storageKey);
        existingUser.PhotoMetadata.SizeBytes.Should().Be(fileSize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]  // Whitespace only
    [InlineData("\t\n")]  // Tabs and newlines
    public async Task Handle_WithWhitespaceOnlyFileName_ShouldThrowDomainException(string fileName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto(fileName, "image/jpeg", "avatars/user123.jpg", 102400);
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("File name cannot be empty.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]  // Whitespace only
    [InlineData("\t\n")]  // Tabs and newlines
    public async Task Handle_WithWhitespaceOnlyStorageKey_ShouldThrowDomainException(string storageKey)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var uploadedPhoto = new UploadedPhotoDto("avatar.jpg", "image/jpeg", storageKey, 102400);
        var command = new UpdateUserAvatarCommand(userId, uploadedPhoto);

        var existingUser = CreateUserMock(userId, "testuser");
        _userRepositoryMock.Setup(repo => repo.GetById(userId, _cancellationToken))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> act = async () => await UpdateUserAvatarRequestHandler.Handle(command, _userRepositoryMock.Object, 
            _photoMetadataRepositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Storage key is required.");

        _userRepositoryMock.Verify(repo => repo.GetById(userId, _cancellationToken), Times.Once);
        _photoMetadataRepositoryMock.Verify(repo => repo.Add(It.IsAny<CoffeePeek.Account.Domain.Entities.PhotoMetadata>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(_cancellationToken), Times.Never);
        _eventPublisherMock.Verify(publisher => publisher.Publish(It.IsAny<PhotoReplacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
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

    private CoffeePeek.Account.Domain.Entities.PhotoMetadata CreatePhotoMetadata(Guid id, string fileName, string contentType, string storageKey, long size)
    {
        // Create PhotoMetadata with reflection to set the ID
        var photo = CoffeePeek.Account.Domain.Entities.PhotoMetadata.Create(fileName, contentType, storageKey, size);
        
        // Set the Id using reflection
        var entityBaseType = typeof(CoffeePeek.Shared.Domain.Entities.Entity<Guid>);
        var idProperty = entityBaseType.GetProperty("Id", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idProperty?.SetValue(photo, id);
        
        return photo;
    }
}