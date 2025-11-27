using CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Moderation;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using FluentAssertions;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.CoffeeShop.Moderation;

public class GetCoffeeShopsInModerationByIdRequestHandlerTests
{
    private readonly Mock<IRepository<ModerationShop>> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCoffeeShopsInModerationByIdRequestHandler _handler;

    public GetCoffeeShopsInModerationByIdRequestHandlerTests()
    {
        // Setup mocks
        _repositoryMock = new Mock<IRepository<ModerationShop>>();
        _mapperMock = new Mock<IMapper>();

        // Create handler instance
        _handler = new GetCoffeeShopsInModerationByIdRequestHandler(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnModerationShops_ForGivenUserId()
    {
        // Arrange
        var request = new GetCoffeeShopsInModerationByIdRequest(1);
        var moderationShops = new List<ModerationShop>
        {
            new() { Id = 1, Name = "Shop 1", UserId = 1 },
            new() { Id = 2, Name = "Shop 2", UserId = 1 }
        };
        
        var moderationShopDtos = new ModerationShopDto[]
        {
            new() { Id = 1, Name = "Shop 1" },
            new() { Id = 2, Name = "Shop 2" }
        };
        
        var queryable = moderationShops.AsQueryable();

        var asyncEnumerable = new TestAsyncEnumerable<ModerationShop>(queryable);

        _repositoryMock
            .Setup(r => r.FindBy(It.IsAny<System.Linq.Expressions.Expression<Func<ModerationShop, bool>>>()))
            .Returns(asyncEnumerable);
            
        _mapperMock
            .Setup(m => m.Map<ModerationShopDto[]>(It.IsAny<IEnumerable<ModerationShop>>()))
            .Returns(moderationShopDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ModerationShop.Should().HaveCount(2);

        _repositoryMock.Verify(r => r.FindBy(It.IsAny<System.Linq.Expressions.Expression<Func<ModerationShop, bool>>>()), Times.Once);
        _mapperMock.Verify(m => m.Map<ModerationShopDto[]>(It.IsAny<IEnumerable<ModerationShop>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResponse_WhenNoModerationShopsExistForUser()
    {
        // Arrange
        var request = new GetCoffeeShopsInModerationByIdRequest(1);
        var moderationShops = new List<ModerationShop>();
        var moderationShopDtos = Array.Empty<ModerationShopDto>();

        var queryable = moderationShops.AsQueryable();

        var asyncEnumerable = new TestAsyncEnumerable<ModerationShop>(queryable);
        
        _repositoryMock
            .Setup(r => r.FindBy(It.IsAny<System.Linq.Expressions.Expression<Func<ModerationShop, bool>>>()))
            .Returns(asyncEnumerable);
            
        _mapperMock
            .Setup(m => m.Map<ModerationShopDto[]>(It.IsAny<IEnumerable<ModerationShop>>()))
            .Returns(moderationShopDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ModerationShop.Should().BeEmpty();

        _repositoryMock.Verify(r => r.FindBy(It.IsAny<System.Linq.Expressions.Expression<Func<ModerationShop, bool>>>()), Times.Once);
        _mapperMock.Verify(m => m.Map<ModerationShopDto[]>(It.IsAny<IEnumerable<ModerationShop>>()), Times.Once);
    }
}