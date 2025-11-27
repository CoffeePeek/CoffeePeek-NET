using System.Linq.Expressions;
using CoffeePeek.BusinessLogic.RequestHandlers;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.User;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.UnitOfWork;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.User;

public class UpdateProfileRequestHandlerTests
{
    private readonly Mock<IUnitOfWork<CoffeePeekDbContext>> _unitOfWorkMock;
    private readonly Mock<DbSet<CoffeePeek.Domain.Entities.Users.User>> _usersDbSetMock;
    private readonly UpdateProfileRequestHandler _handler;

    public UpdateProfileRequestHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork<CoffeePeekDbContext>>();
        _usersDbSetMock = new Mock<DbSet<Domain.Entities.Users.User>>();

        _unitOfWorkMock.Setup(uow => uow.DbContext.Set<Domain.Entities.Users.User>()).Returns(_usersDbSetMock.Object);

        _handler = new UpdateProfileRequestHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            UserId = 999,
            PhotoUrl = "New Name",
            About = "New About"
        };
        CoffeePeek.Domain.Entities.Users.User? user = null;

        _usersDbSetMock
            .Setup(u => u.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CoffeePeek.Domain.Entities.Users.User, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");

        _usersDbSetMock.Verify(u => u.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CoffeePeek.Domain.Entities.Users.User, bool>>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
        
        _unitOfWorkMock.Verify(uow => uow.DbContext.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProfileIsUpdated()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            UserId = 1,
            PhotoUrl = "New Name",
            About = "New About"
        };
        var user = new CoffeePeek.Domain.Entities.Users.User 
        { 
            Id = 1, 
            UserName = "Old Name", 
            About = "Old About" 
        };

        _usersDbSetMock
            .Setup(u => u.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CoffeePeek.Domain.Entities.Users.User, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile updated successfully");

        _usersDbSetMock.Verify(u => u.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CoffeePeek.Domain.Entities.Users.User, bool>>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
        
        user.UserName.Should().Be("New Name");
        user.About.Should().Be("New About");
        
        _unitOfWorkMock.Verify(uow => uow.DbContext.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOnlyProvidedFields_WhenSomeFieldsAreNull()
    {
        // Arrange
        var request = new UpdateProfileRequest
        {
            UserId = 1,
            PhotoUrl = "New Name",
            About = null
        };
        var user = new CoffeePeek.Domain.Entities.Users.User 
        { 
            Id = 1, 
            UserName = "Old Name", 
            About = "Old About" 
        };

        _usersDbSetMock
            .Setup(u => u.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Domain.Entities.Users.User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.DbContext.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _usersDbSetMock.Verify(u => u.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CoffeePeek.Domain.Entities.Users.User, bool>>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
        
        user.UserName.Should().Be("New Name");
        user.About.Should().Be("Old About"); // Should remain unchanged
        
        _unitOfWorkMock.Verify(uow => uow.DbContext.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}