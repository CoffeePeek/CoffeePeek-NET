using CoffeePeek.BusinessLogic.RequestHandlers;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.UnitOfWork;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.User;

public class DeleteUserRequestHandlerTests
{
    private readonly Mock<IUnitOfWork<CoffeePeekDbContext>> _unitOfWorkMock;
    private readonly Mock<DbSet<CoffeePeek.Domain.Entities.Users.User>> _usersDbSetMock;
    private readonly DeleteUserRequestHandler _handler;

    public DeleteUserRequestHandlerTests()
    {
        // Setup mocks
        _unitOfWorkMock = new Mock<IUnitOfWork<CoffeePeekDbContext>>();
        _usersDbSetMock = new Mock<DbSet<CoffeePeek.Domain.Entities.Users.User>>();

        // Setup DbContext mock
        var dbContextMock = new Mock<CoffeePeekDbContext>();
        dbContextMock.Setup(db => db.Users).Returns(_usersDbSetMock.Object);
        _unitOfWorkMock.Setup(uow => uow.DbContext).Returns(dbContextMock.Object);

        // Create handler instance
        _handler = new DeleteUserRequestHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        var request = new DeleteUserRequest(999);
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
    public async Task Handle_ShouldReturnSuccess_WhenUserIsSoftDeleted()
    {
        // Arrange
        var request = new DeleteUserRequest(1);
        var user = new CoffeePeek.Domain.Entities.Users.User { Id = 1, UserName = "Test User", IsSoftDeleted = false };

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
        result.Data.Should().BeTrue();

        _usersDbSetMock.Verify(u => u.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CoffeePeek.Domain.Entities.Users.User, bool>>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
        
        user.IsSoftDeleted.Should().BeTrue();
        
        _unitOfWorkMock.Verify(uow => uow.DbContext.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}