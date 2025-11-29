using System.Linq.Expressions;
using CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Review;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Shop;
using MapsterMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.RequestHandlers.CoffeeShop.Review;

public class GetReviewByIdRequestHandlerTests
{
    private readonly Mock<DbSet<Domain.Entities.Review.Review>> _reviewsDbSetMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetReviewByIdRequestHandler _handler;

    public GetReviewByIdRequestHandlerTests()
    {
        var dbContextMock =
            // Setup mocks
            new Mock<CoffeePeekDbContext>();
        _reviewsDbSetMock = new Mock<DbSet<Domain.Entities.Review.Review>>();
        _mapperMock = new Mock<IMapper>();

        // Setup DbContext mock
        dbContextMock.Setup(db => db.Reviews).Returns(_reviewsDbSetMock.Object);

        // Create handler instance
        _handler = new GetReviewByIdRequestHandler(dbContextMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenReviewNotFound()
    {
        // Arrange
        var request = new GetReviewByIdRequest(999);
        Domain.Entities.Review.Review? review = null;

        var reviews = new List<Domain.Entities.Review.Review>().AsQueryable();

        var mockSet = new Mock<DbSet<Domain.Entities.Review.Review>>();
        mockSet.As<IQueryable<Domain.Entities.Review.Review>>().Setup(m => m.Provider).Returns(reviews.Provider);
        mockSet.As<IQueryable<Domain.Entities.Review.Review>>().Setup(m => m.Expression).Returns(reviews.Expression);
        mockSet.As<IQueryable<Domain.Entities.Review.Review>>().Setup(m => m.ElementType).Returns(reviews.ElementType);
        mockSet.As<IQueryable<Domain.Entities.Review.Review>>().Setup(m => m.GetEnumerator()).Returns(reviews.GetEnumerator());

        mockSet.Setup(m => m.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Domain.Entities.Review.Review, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Review.Review?)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Review not found");

        _reviewsDbSetMock.Verify(r => r.AsNoTracking(), Times.Once);
        _reviewsDbSetMock.Verify(r => r.Include(It.IsAny<Expression<Func<Domain.Entities.Review.Review, object>>>()), Times.Once);
        _reviewsDbSetMock.Verify(r => r.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Domain.Entities.Review.Review, bool>>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
            
        _mapperMock.Verify(m => m.Map<CoffeeShopReviewDto>(It.IsAny<Domain.Entities.Review.Review>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenReviewIsFound()
    {
        // Arrange
        var request = new GetReviewByIdRequest(1);
        var review = new Domain.Entities.Review.Review 
        { 
            Id = 1, 
            Header = "Great Coffee", 
            Comment = "Loved it!",
            Shop = new Shop { Id = 1, Name = "Coffee Shop" }
        };
        
        var reviewDto = new CoffeeShopReviewDto 
        { 
            Id = 1, 
            Header = "Great Coffee", 
            Comment = "Loved it!",
            ShopName = "Coffee Shop"
        };

        var reviews = new List<Domain.Entities.Review.Review>().AsQueryable();

        var mockSet = new Mock<DbSet<Domain.Entities.Review.Review>>();
        mockSet.As<IQueryable<Domain.Entities.Review.Review>>().Setup(m => m.Provider).Returns(reviews.Provider);
        mockSet.As<IQueryable<Domain.Entities.Review.Review>>().Setup(m => m.Expression).Returns(reviews.Expression);
        mockSet.As<IQueryable<Domain.Entities.Review.Review>>().Setup(m => m.ElementType).Returns(reviews.ElementType);
        mockSet.As<IQueryable<Domain.Entities.Review.Review>>().Setup(m => m.GetEnumerator()).Returns(reviews.GetEnumerator());

        mockSet.Setup(m => m.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Domain.Entities.Review.Review, bool>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Review.Review?)null);
            
        _mapperMock
            .Setup(m => m.Map<CoffeeShopReviewDto>(review))
            .Returns(reviewDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Review.Should().NotBeNull();
        result.Data.Review.Id.Should().Be(1);
        result.Data.Review.Header.Should().Be("Great Coffee");

        _reviewsDbSetMock.Verify(r => r.AsNoTracking(), Times.Once);
        _reviewsDbSetMock.Verify(r => r.Include(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Review.Review, object>>>()), Times.Once);
        _reviewsDbSetMock.Verify(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Review.Review, bool>>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
            
        _mapperMock.Verify(m => m.Map<CoffeeShopReviewDto>(review), Times.Once);
    }
}