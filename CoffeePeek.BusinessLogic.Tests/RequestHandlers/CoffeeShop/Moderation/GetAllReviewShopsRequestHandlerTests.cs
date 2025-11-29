using CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Moderation;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using MapsterMapper;
using FluentAssertions;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.CoffeeShop.Moderation;

public class GetAllReviewShopsRequestHandlerTests
{
    private readonly Mock<IRepository<ModerationShop>> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllReviewShopsRequestHandler _handler;

    public GetAllReviewShopsRequestHandlerTests()
    {
        // Setup mocks
        _repositoryMock = new Mock<IRepository<ModerationShop>>();
        _mapperMock = new Mock<IMapper>();

        // Create handler instance
        _handler = new GetAllReviewShopsRequestHandler(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllModerationShops()
    {
        // Arrange
        var request = new GetAllModerationShopsRequest();
        var moderationShops = new List<ModerationShop>
        {
            new() { Id = 1, Name = "Shop 1" },
            new() { Id = 2, Name = "Shop 2" }
        };
        
        var moderationShopDtos = new ModerationShopDto[]
        {
            new() { Id = 1, Name = "Shop 1" },
            new() { Id = 2, Name = "Shop 2" }
        };
        
        var queryable = moderationShops.AsQueryable();

        var asyncEnumerable = new TestAsyncEnumerable<ModerationShop>(queryable);

        _repositoryMock
            .Setup(r => r.GetAll())
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

        _repositoryMock.Verify(r => r.GetAll(), Times.Once);
        _mapperMock.Verify(m => m.Map<ModerationShopDto[]>(It.IsAny<IEnumerable<ModerationShop>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResponse_WhenNoModerationShopsExist()
    {
        // Arrange
        var request = new GetAllModerationShopsRequest();
        var moderationShops = new List<ModerationShop>();
        var moderationShopDtos = Array.Empty<ModerationShopDto>();
        
        var queryable = moderationShops.AsQueryable();

        var asyncEnumerable = new TestAsyncEnumerable<ModerationShop>(queryable);
        
        _repositoryMock
            .Setup(r => r.GetAll())
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

        _repositoryMock.Verify(r => r.GetAll(), Times.Once);
        _mapperMock.Verify(m => m.Map<ModerationShopDto[]>(It.IsAny<IEnumerable<ModerationShop>>()), Times.Once);
    }
}