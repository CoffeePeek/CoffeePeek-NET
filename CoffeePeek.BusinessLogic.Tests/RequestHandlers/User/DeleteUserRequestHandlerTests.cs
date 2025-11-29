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
    private readonly DeleteUserRequestHandler _handler;
    private readonly CoffeePeekDbContext _dbContext;

    public DeleteUserRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<CoffeePeekDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CoffeePeekDbContext(options);

        var unitOfWorkMock = new Mock<IUnitOfWork<CoffeePeekDbContext>>();
        unitOfWorkMock.Setup(u => u.DbContext).Returns(_dbContext);

        var unitOfWork = unitOfWorkMock.Object;

        _handler = new DeleteUserRequestHandler(unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        var request = new DeleteUserRequest(999); // несуществующий пользователь

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsSoftDeleted()
    {
        // Arrange
        var user = new CoffeePeek.Domain.Entities.Users.User
        {
            Id = 1,
            UserName = "Test User",
            IsSoftDeleted = false
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new DeleteUserRequest(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        user.IsSoftDeleted.Should().BeTrue();

        var userInDb = await _dbContext.Users.FindAsync(1);
        userInDb!.IsSoftDeleted.Should().BeTrue();
    }
}