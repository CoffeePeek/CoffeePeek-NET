using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain.Entities;
using FluentAssertions;
using JetBrains.Annotations;

namespace CoffeePeek.Shops.Domain.Tests.Entities;

[TestSubject(typeof(Rating))]
public class RatingTests
{
    [Fact]
    public void Create_WithCoffeeBelowMin_ThrowsDomainException()
    {
        // Act — coffee=0, below MinReviewRate=1
        Action act = () => Rating.Create(3, 3, 0);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithCoffeeAboveMax_ThrowsDomainException()
    {
        // Act — coffee=6, above MaxReviewRate=5
        Action act = () => Rating.Create(3, 3, 6);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithPlaceBelowMin_ThrowsDomainException()
    {
        // Act — place=0, below MinReviewRate=1
        Action act = () => Rating.Create(0, 3, 3);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithPlaceAboveMax_ThrowsDomainException()
    {
        // Act — place=6, above MaxReviewRate=5
        Action act = () => Rating.Create(6, 3, 3);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithServiceBelowMin_ThrowsDomainException()
    {
        // Act — service=0, below MinReviewRate=1
        Action act = () => Rating.Create(3, 0, 3);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithServiceAboveMax_ThrowsDomainException()
    {
        // Act — service=6, above MaxReviewRate=5
        Action act = () => Rating.Create(3, 6, 3);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithAllValidValues_ReturnsRating()
    {
        // Act
        var rating = Rating.Create(3, 4, 5);

        // Assert
        rating.Place.Should().Be(3);
        rating.Service.Should().Be(4);
        rating.Coffee.Should().Be(5);
        rating.AverageRating.Should().Be((3m + 4m + 5m) / 3m);
    }
}
