using CoffeePeek.BuildingBlocks.Mapper;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Domain.Entities.Review;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Entities.Users;
using FluentAssertions;
using Mapster;

namespace CoffeePeek.BuildingBlocks.Tests.Mapper;

public class MapsterConfigTests
{
    private readonly TypeAdapterConfig _config;

    public MapsterConfigTests()
    {
        _config = new TypeAdapterConfig();
        var mapsterConfig = new MapsterConfig();
        mapsterConfig.Register(_config);
    }

    [Fact]
    public void MapsterConfig_ShouldImplementIRegister()
    {
        // Arrange & Act
        var config = new MapsterConfig();

        // Assert
        config.Should().BeAssignableTo<IRegister>();
    }

    [Fact]
    public void MapsterConfig_Register_ShouldNotThrow()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        var mapsterConfig = new MapsterConfig();

        // Act
        Action act = () => mapsterConfig.Register(config);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SendCoffeeShopToModerationRequest_ToModerationShop_ShouldMapNotValidatedAddress()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationRequest
        {
            NotValidatedAddress = "123 Main St"
        };

        // Act
        var moderationShop = request.Adapt<ModerationShop>(_config);

        // Assert
        moderationShop.Should().NotBeNull();
        moderationShop.NotValidatedAddress.Should().Be("123 Main St");
    }

    [Fact]
    public void Review_ToCoffeeShopReviewDto_ShouldMapShopName_WhenShopIsNotNull()
    {
        // Arrange
        var review = new Review
        {
            Id = 1,
            Shop = new Shop
            {
                Id = 1,
                Name = "Coffee Paradise"
            }
        };

        // Act
        var dto = review.Adapt<CoffeeShopReviewDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.ShopName.Should().Be("Coffee Paradise");
    }

    [Fact]
    public void Review_ToCoffeeShopReviewDto_ShouldMapEmptyShopName_WhenShopIsNull()
    {
        // Arrange
        var review = new Review
        {
            Id = 1,
            Shop = null
        };

        // Act
        var dto = review.Adapt<CoffeeShopReviewDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.ShopName.Should().BeEmpty();
    }

    [Fact]
    public void User_ToUserDto_ShouldMapReviewCount_WhenReviewsExist()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Reviews = new List<Review>
            {
                new Review { Id = 1 },
                new Review { Id = 2 },
                new Review { Id = 3 }
            }
        };

        // Act
        var dto = user.Adapt<UserDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.ReviewCount.Should().Be(3);
    }

    [Fact]
    public void User_ToUserDto_ShouldMapZeroReviewCount_WhenReviewsIsNull()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Reviews = null
        };

        // Act
        var dto = user.Adapt<UserDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.ReviewCount.Should().Be(0);
    }

    [Fact]
    public void User_ToUserDto_ShouldMapZeroReviewCount_WhenReviewsIsEmpty()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Reviews = new List<Review>()
        };

        // Act
        var dto = user.Adapt<UserDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.ReviewCount.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void User_ToUserDto_ShouldMapCorrectReviewCount(int reviewCount)
    {
        // Arrange
        var reviews = Enumerable.Range(1, reviewCount)
            .Select(i => new Review { Id = i })
            .ToList();

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Reviews = reviews
        };

        // Act
        var dto = user.Adapt<UserDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.ReviewCount.Should().Be(reviewCount);
    }

    [Fact]
    public void SendCoffeeShopToModerationRequest_ToModerationShop_ShouldHandleNullAddress()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationRequest
        {
            NotValidatedAddress = null
        };

        // Act
        var moderationShop = request.Adapt<ModerationShop>(_config);

        // Assert
        moderationShop.Should().NotBeNull();
        moderationShop.NotValidatedAddress.Should().BeNull();
    }

    [Fact]
    public void SendCoffeeShopToModerationRequest_ToModerationShop_ShouldHandleEmptyAddress()
    {
        // Arrange
        var request = new SendCoffeeShopToModerationRequest
        {
            NotValidatedAddress = ""
        };

        // Act
        var moderationShop = request.Adapt<ModerationShop>(_config);

        // Assert
        moderationShop.Should().NotBeNull();
        moderationShop.NotValidatedAddress.Should().BeEmpty();
    }

    [Fact]
    public void Review_ToCoffeeShopReviewDto_ShouldMapAllProperties()
    {
        // Arrange
        var review = new Review
        {
            Id = 1,
            Shop = new Shop
            {
                Id = 5,
                Name = "Best Coffee"
            }
        };

        // Act
        var dto = review.Adapt<CoffeeShopReviewDto>(_config);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.ShopName.Should().Be("Best Coffee");
    }
}
