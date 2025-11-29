using CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Review;
using MapsterMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.CoffeeShop.Review;

public class GetAllReviewsRequestHandlerTests
{
    private readonly Mock<DbSet<Domain.Entities.Review.Review>> _reviewsDbSetMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllReviewsRequestHandler _handler;

    public GetAllReviewsRequestHandlerTests()
    {
        var dbContextMock = new Mock<CoffeePeekDbContext>();
        _reviewsDbSetMock = new Mock<DbSet<Domain.Entities.Review.Review>>();
        _mapperMock = new Mock<IMapper>();

        // Setup DbContext mock
        dbContextMock.Setup(db => db.Reviews).Returns(_reviewsDbSetMock.Object);

        // Create handler instance
        _handler = new GetAllReviewsRequestHandler(dbContextMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllReviews()
    {
        // Arrange
        var request = new GetAllReviewsRequest(1);
        var reviews = new List<Domain.Entities.Review.Review>
        {
            new() { Id = 1, Header = "Great Coffee", Comment = "Loved it!" },
            new() { Id = 2, Header = "Good Service", Comment = "Nice place" }
        };

        var reviewDtos = new CoffeeShopReviewDto[]
        {
            new() { Id = 1, Header = "Great Coffee", Comment = "Loved it!" },
            new() { Id = 2, Header = "Good Service", Comment = "Nice place" }
        };

        _reviewsDbSetMock
            .Setup(r => r.AsNoTracking())
            .Returns(_reviewsDbSetMock.Object);

        _reviewsDbSetMock
            .Setup(r => r.ToArrayAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews.ToArray());

        _mapperMock
            .Setup(m => m.Map<CoffeeShopReviewDto[]>(It.IsAny<IEnumerable<Domain.Entities.Review.Review>>()))
            .Returns(reviewDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Reviews.Should().HaveCount(2);

        _reviewsDbSetMock.Verify(r => r.AsNoTracking(), Times.Once);
        _reviewsDbSetMock.Verify(r => r.ToArrayAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<CoffeeShopReviewDto[]>(It.IsAny<IEnumerable<Domain.Entities.Review.Review>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResponse_WhenNoReviewsExist()
    {
        // Arrange
        var request = new GetAllReviewsRequest(1);
        var reviews = new List<Domain.Entities.Review.Review>();
        var reviewDtos = Array.Empty<CoffeeShopReviewDto>();

        _reviewsDbSetMock
            .Setup(r => r.AsNoTracking())
            .Returns(_reviewsDbSetMock.Object);

        _reviewsDbSetMock
            .Setup(r => r.ToArrayAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews.ToArray());

        _mapperMock
            .Setup(m => m.Map<CoffeeShopReviewDto[]>(It.IsAny<IEnumerable<Domain.Entities.Review.Review>>()))
            .Returns(reviewDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Reviews.Should().BeEmpty();

        _reviewsDbSetMock.Verify(r => r.AsNoTracking(), Times.Once);
        _reviewsDbSetMock.Verify(r => r.ToArrayAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<CoffeeShopReviewDto[]>(It.IsAny<IEnumerable<Domain.Entities.Review.Review>>()),
            Times.Once);
    }
}