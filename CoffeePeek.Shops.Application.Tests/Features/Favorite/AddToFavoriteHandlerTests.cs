using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Features.Favorite.AddToFavorite;
using CoffeePeek.Shops.Domain.Aggregates.UserFavoriteAggregate;
using FluentAssertions;
using Moq;
using SharedValidationResult = CoffeePeek.Shared.Validation.ValidationResult;

namespace CoffeePeek.Shops.Application.Tests.Features.Favorite.AddToFavorite;

public class AddToFavoriteHandlerTests
{
    private readonly Mock<IUserFavoriteService> _favoriteServiceMock = new();
    private readonly Mock<IValidationStrategy<AddToFavoriteCommand>> _validationMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    [Fact]
    public async Task Handle_WithValidCommand_AddsToFavoritesAndSaves()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var command = new AddToFavoriteCommand(userId, shopId);
        var expectedId = Guid.NewGuid();

        _validationMock.Setup(v => v.Validate(command)).Returns(SharedValidationResult.Valid);
        _favoriteServiceMock.Setup(s => s.AddToFavoritesAsync(userId, shopId, _ct)).ReturnsAsync(expectedId);

        var result = await AddToFavoriteHandler.Handle(
            command,
            _favoriteServiceMock.Object,
            _validationMock.Object,
            _unitOfWorkMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedId);
        _favoriteServiceMock.Verify(s => s.AddToFavoritesAsync(userId, shopId, _ct), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsValidationException()
    {
        var command = new AddToFavoriteCommand(Guid.NewGuid(), Guid.Empty);

        _validationMock.Setup(v => v.Validate(command))
            .Returns(SharedValidationResult.Invalid("CoffeeShopId is required and cannot be empty"));

        Func<Task> act = async () => await AddToFavoriteHandler.Handle(
            command,
            _favoriteServiceMock.Object,
            _validationMock.Object,
            _unitOfWorkMock.Object,
            _ct);

        await act.Should().ThrowAsync<ValidationException>();
        _favoriteServiceMock.Verify(
            s => s.AddToFavoritesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
