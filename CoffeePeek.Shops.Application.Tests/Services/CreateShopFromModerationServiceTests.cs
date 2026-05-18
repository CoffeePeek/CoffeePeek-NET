using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shops.Application.Services;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CoffeePeek.Shops.Application.Tests.Services;

public class CreateShopFromModerationServiceTests
{
    private readonly Mock<IQueryCoffeeShopRepository> _shopRepoMock = new();
    private readonly Mock<IQueryCoffeeBeanRepository> _coffeeBeanRepoMock = new();
    private readonly Mock<IQueryEquipmentRepository> _equipmentRepoMock = new();
    private readonly Mock<IQueryRoasterRepository> _roasterRepoMock = new();
    private readonly Mock<IQueryBrewMethodRepository> _brewMethodRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<CreateShopFromModerationService>> _loggerMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private CreateShopFromModerationService CreateSut() =>
        new CreateShopFromModerationService(
            _shopRepoMock.Object,
            _coffeeBeanRepoMock.Object,
            _equipmentRepoMock.Object,
            _roasterRepoMock.Object,
            _brewMethodRepoMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);

    private static ShopDto CreateMinimalShopDto() =>
        new ShopDto
        {
            Name = "Test Shop",
            Description = null,
            PriceRange = CoffeePeek.Contract.Enums.PriceRange.Moderate,
            CityId = Guid.NewGuid(),
            Location = null,
            ShopContact = null,
            Equipments = null,
            BrewMethods = null,
            Roasters = null,
            CoffeeBeans = null,
            Schedules = null,
            Photos = [],
            Reviews = []
        };

    [Fact]
    public async Task CreateShopFromApprovedEventAsync_WhenModerationIdIsNew_AddsShopAndSavesExactlyOnce()
    {
        // Arrange
        _shopRepoMock
            .Setup(r => r.ExistsByModerationId(It.IsAny<Guid>(), _ct))
            .ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var result = await sut.CreateShopFromApprovedEventAsync(
            CreateMinimalShopDto(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _ct);

        // Assert — BUG-05 regression: SaveChangesAsync must be called exactly once after Add
        result.Should().NotBe(Guid.Empty);
        _shopRepoMock.Verify(r => r.Add(It.IsAny<CoffeeShop>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task CreateShopFromApprovedEventAsync_WhenModerationIdAlreadyExists_ThrowsAndDoesNotAddOrSave()
    {
        // Arrange
        _shopRepoMock
            .Setup(r => r.ExistsByModerationId(It.IsAny<Guid>(), _ct))
            .ReturnsAsync(true);

        var sut = CreateSut();

        // Act
        Func<Task> act = () => sut.CreateShopFromApprovedEventAsync(
            CreateMinimalShopDto(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _ct);

        // Assert — duplicate guard must throw before Add or SaveChangesAsync are reached
        await act.Should().ThrowAsync<InvalidOperationException>();
        _shopRepoMock.Verify(r => r.Add(It.IsAny<CoffeeShop>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
