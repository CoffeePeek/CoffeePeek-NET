using System;
using System.Threading;
using System.Threading.Tasks;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using FluentAssertions;
using MapsterMapper;
using Moq;
using Wolverine;

namespace CoffeePeek.Shops.Application.Tests.Features.CheckIn.CreateCheckIn;

public class CreateCheckInHandlerTests
{
    private readonly Mock<IQueryCheckInRepository> _checkInRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMessageBus> _busMock = new();
    private readonly Mock<IAsyncValidationStrategy<CreateCheckInCommand>> _validationMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CancellationToken _ct = CancellationToken.None;

    private static CreateCheckInCommand BuildCommand(
        bool isPublic,
        RatingDto? rating = null,
        string note = "Some valid note here for testing purposes") =>
        new CreateCheckInCommand(
            CoffeeShopId: Guid.NewGuid(),
            IsPublic: isPublic,
            VisitedAt: DateTime.UtcNow.AddHours(-1),
            Note: note,
            Photos: null,
            Rating: rating)
        { UserId = Guid.NewGuid(), UserName = "testuser" };

    [Fact]
    public async Task Handle_PrivateCheckIn_CreatesCheckInAndSaves()
    {
        var command = BuildCommand(isPublic: false);
        _validationMock.Setup(v => v.ValidateAsync(command, _ct))
            .ReturnsAsync(ValidationResult.Valid);

        var result = await CreateCheckInHandler.Handle(
            command,
            _checkInRepoMock.Object,
            _unitOfWorkMock.Object,
            _busMock.Object,
            _validationMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        _busMock.Verify(b => b.PublishAsync(It.IsAny<object>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task Handle_PublicCheckIn_WithValidRating_PublishesEventAndSaves()
    {
        var rating = new RatingDto { Place = 3, Service = 4, Coffee = 5 };
        var command = BuildCommand(
            isPublic: true,
            rating: rating,
            note: "This is a sufficiently long note for review comment");

        _validationMock.Setup(v => v.ValidateAsync(command, _ct))
            .ReturnsAsync(ValidationResult.Valid);
        _busMock.Setup(b => b.PublishAsync(It.IsAny<object>()))
            .Returns(ValueTask.CompletedTask);
        _mapperMock.Setup(m => m.Map<CoffeePeek.Contract.Dtos.CoffeeShop.ReviewDto>(It.IsAny<object>()))
            .Returns(new CoffeePeek.Contract.Dtos.CoffeeShop.ReviewDto());

        var result = await CreateCheckInHandler.Handle(
            command,
            _checkInRepoMock.Object,
            _unitOfWorkMock.Object,
            _busMock.Object,
            _validationMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _ct);

        result.IsSuccess.Should().BeTrue();
        _busMock.Verify(b => b.PublishAsync(It.IsAny<object>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task Handle_PublicCheckIn_WithInvalidRating_ThrowsDomainException()
    {
        // Place=0 violates MinReviewRate=1 in Rating.Create — DomainException must propagate
        var rating = new RatingDto { Place = 0, Service = 3, Coffee = 3 };
        var note = "This note is long enough to serve as both a header and comment in the review";
        var command = BuildCommand(isPublic: true, rating: rating, note: note);

        // Application-level validation passes (mocked) but domain validation fails
        _validationMock.Setup(v => v.ValidateAsync(command, _ct))
            .ReturnsAsync(ValidationResult.Valid);

        Func<Task> act = async () => await CreateCheckInHandler.Handle(
            command,
            _checkInRepoMock.Object,
            _unitOfWorkMock.Object,
            _busMock.Object,
            _validationMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _ct);

        // TEST-04 regression: DomainException must NOT be swallowed by catch block
        await act.Should().ThrowAsync<DomainException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ThrowsValidationException()
    {
        var command = BuildCommand(isPublic: false);
        _validationMock.Setup(v => v.ValidateAsync(command, _ct))
            .ReturnsAsync(ValidationResult.Invalid("CoffeeShopId is required"));

        Func<Task> act = async () => await CreateCheckInHandler.Handle(
            command,
            _checkInRepoMock.Object,
            _unitOfWorkMock.Object,
            _busMock.Object,
            _validationMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _ct);

        await act.Should().ThrowAsync<CoffeePeek.Shared.Kernel.Exceptions.ValidationException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
