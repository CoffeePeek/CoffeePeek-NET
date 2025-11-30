using CoffeePeek.BusinessLogic.RequestHandlers;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.User;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.User;

public class GetAllUserRequestHandlerTests
{
    private readonly Mock<IUnitOfWork<CoffeePeekDbContext>> _unitOfWorkMock;
    private readonly Mock<IRepository<Domain.Entities.Users.User>> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllUserRequestHandler _handler;

    public GetAllUserRequestHandlerTests()
    {
        // Setup mocks
        _unitOfWorkMock = new Mock<IUnitOfWork<CoffeePeekDbContext>>();
        _repositoryMock = new Mock<IRepository<Domain.Entities.Users.User>>();
        _mapperMock = new Mock<IMapper>();

        // Setup UnitOfWork mock
        _unitOfWorkMock.Setup(uow => uow.GetRepository<Domain.Entities.Users.User>()).Returns(_repositoryMock.Object);

        // Create handler instance
        _handler = new GetAllUserRequestHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUsers()
    {
        // Arrange
        var request = new GetAllUsersRequest();
        var users = new List<Domain.Entities.Users.User>
        {
            new() { Id = 1, UserName = "User 1", Email = "user1@example.com" },
            new() { Id = 2, UserName = "User 2", Email = "user2@example.com" }
        };

        var userDtos = new UserDto[]
        {
            new() { Id = 1, UserName = "User 1", Email = "user1@example.com" },
            new() { Id = 2, UserName = "User 2", Email = "user2@example.com" }
        };

        var options = new DbContextOptionsBuilder<CoffeePeekDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new CoffeePeekDbContext(options);
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var unitOfWorkMock = new Mock<IUnitOfWork<CoffeePeekDbContext>>();
        unitOfWorkMock.Setup(u => u.DbContext).Returns(dbContext);

        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(m => m.Map<UserDto[]>(It.IsAny<IEnumerable<Domain.Entities.Users.User>>()))
            .Returns(userDtos);

        var handler = new GetAllUserRequestHandler(unitOfWorkMock.Object, mapperMock.Object);

// Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);

        _unitOfWorkMock.Verify(uow => uow.GetRepository<Domain.Entities.Users.User>(), Times.Once);
        _repositoryMock.Verify(r => r.GetAll(), Times.Once);
        _mapperMock.Verify(m => m.Map<UserDto[]>(It.IsAny<IEnumerable<Domain.Entities.Users.User>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyArray_WhenNoUsersExist()
    {
        // Arrange
        var request = new GetAllUsersRequest();
        var users = new List<Domain.Entities.Users.User>();
        var userDtos = Array.Empty<UserDto>();

        _repositoryMock
            .Setup(r => r.GetAll())
            .Returns(users.AsQueryable());

        _mapperMock
            .Setup(m => m.Map<UserDto[]>(It.IsAny<IEnumerable<Domain.Entities.Users.User>>()))
            .Returns(userDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();

        _unitOfWorkMock.Verify(uow => uow.GetRepository<Domain.Entities.Users.User>(), Times.Once);
        _repositoryMock.Verify(r => r.GetAll(), Times.Once);
        _mapperMock.Verify(m => m.Map<UserDto[]>(It.IsAny<IEnumerable<Domain.Entities.Users.User>>()), Times.Once);
    }
}