using CoffeePeek.BusinessLogic.RequestHandlers;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Contract.Response;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Review;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.UnitOfWork;
using FluentAssertions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.User;

public class GetProfileRequestHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork<CoffeePeekDbContext>> _unitOfWorkMock;
    private readonly Mock<DbSet<CoffeePeek.Domain.Entities.Users.User>> _usersDbSetMock;
    private readonly GetProfileRequestHandler _handler;

    public GetProfileRequestHandlerTests()
    {
        // Setup mocks
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork<CoffeePeekDbContext>>();
        _usersDbSetMock = new Mock<DbSet<CoffeePeek.Domain.Entities.Users.User>>();

        // Setup DbContext mock
        var dbContextMock = new Mock<CoffeePeekDbContext>();
        dbContextMock.Setup(db => db.Users).Returns(_usersDbSetMock.Object);
        _unitOfWorkMock.Setup(uow => uow.DbContext).Returns(dbContextMock.Object);

        // Create handler instance
        _handler = new GetProfileRequestHandler(
            _mapperMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var request = new GetProfileRequest(userId);
        
        var user = new CoffeePeek.Domain.Entities.Users.User
        {
            Id = userId,
            UserName = "Test User",
            Email = "test@example.com",
            About = "About me"
        };
        
        var userDto = new UserDto
        {
            Id = userId,
            UserName = "Test User",
            Email = "test@example.com",
            Password = "password",
            About = "About me",
            ReviewCount = 0
        };

        // Setup mock for FirstOrDefaultAsync
        var users = new List<CoffeePeek.Domain.Entities.Users.User> { user }.AsQueryable();
        _usersDbSetMock.As<IQueryable<CoffeePeek.Domain.Entities.Users.User>>().Setup(m => m.Provider).Returns(users.Provider);
        _usersDbSetMock.As<IQueryable<CoffeePeek.Domain.Entities.Users.User>>().Setup(m => m.Expression).Returns(users.Expression);
        _usersDbSetMock.As<IQueryable<CoffeePeek.Domain.Entities.Users.User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _usersDbSetMock.As<IQueryable<CoffeePeek.Domain.Entities.Users.User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _mapperMock
            .Setup(m => m.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
        result.Data.UserName.Should().Be("Test User");

        _unitOfWorkMock.Verify(
            uow => uow.DbContext.Users.AsNoTracking(), 
            Times.Once);

        _mapperMock.Verify(
            m => m.Map<UserDto>(user),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = 999;
        var request = new GetProfileRequest(userId);

        // Setup mock for FirstOrDefaultAsync - return null
        var users = new List<CoffeePeek.Domain.Entities.Users.User>().AsQueryable();
        _usersDbSetMock.As<IQueryable<CoffeePeek.Domain.Entities.Users.User>>().Setup(m => m.Provider).Returns(users.Provider);
        _usersDbSetMock.As<IQueryable<CoffeePeek.Domain.Entities.Users.User>>().Setup(m => m.Expression).Returns(users.Expression);
        _usersDbSetMock.As<IQueryable<CoffeePeek.Domain.Entities.Users.User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _usersDbSetMock.As<IQueryable<CoffeePeek.Domain.Entities.Users.User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);
        
        await act.Should().ThrowAsync<BusinessLogic.Exceptions.NotFoundException>()
            .WithMessage("User not found.");

        _unitOfWorkMock.Verify(
            uow => uow.DbContext.Users.AsNoTracking(), 
            Times.Once);

        _mapperMock.Verify(
            m => m.Map<UserDto>(It.IsAny<CoffeePeek.Domain.Entities.Users.User>()),
            Times.Never);
    }
}