using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Entities;
using MapsterMapper;
using Wolverine;
using Wolverine.Attributes;

namespace CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;

using Review = Domain.Aggregates.ReviewAggregate.Review;

public static class CreateCheckInHandler
{
    public static async Task<Response<CreateCheckInResponse>> Handle(
        CreateCheckInCommand command,
        IQueryCheckInRepository queryCheckInRepository,
        IUnitOfWork unitOfWork,
        IMessageBus bus,
        IAsyncValidationStrategy<CreateCheckInCommand> validationStrategy,
        IMapper mapper,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var validationResult = await validationStrategy.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.ErrorMessage!);

        var checkIn = Domain.Aggregates.CheckInAggregate.CheckIn.Create(
            command.UserId, 
            command.CoffeeShopId, 
            command.VisitedAt);

        if (!string.IsNullOrEmpty(command.Note))
            checkIn.UpdateNote(command.Note);    

        if (command.Photos is { Count: > 0 })
        {
            var photos = command.Photos.Select(x =>
                new ShopPhoto(x.FileName, x.ContentType, x.StorageKey, x.Size, command.UserId));
            checkIn.AddPhotos(photos);
        }

        if (command.IsPublic)
        {
            queryCheckInRepository.Add(checkIn);

            try
            {
                var commentPreview = string.Join(" ",
                    command.Note?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(3) ?? []);

                var review = Review.Create(
                    command.CoffeeShopId,
                    command.UserId, 
                    command.UserName,
                    header: commentPreview,
                    comment: command.Note!,
                    ratingPlace: command.Rating!.Place, 
                    ratingService: command.Rating.Service, 
                    ratingCoffee: command.Rating.Coffee);

                await bus.PublishAsync(new CheckinCreatedEvent
                {
                    UserId = command.UserId,
                    ShopId = command.CoffeeShopId,
                    CreatedAt = checkIn.CreatedAtUtc,
                    ReviewDto = mapper.Map<ReviewDto>(review)
                });
            }
            // TEST-04: rethrow explicitly so DomainException is not swallowed by the outer try/catch
            catch (DomainException)
            {
                throw;
            }
        }

        await unitOfWork.SaveChangesAsync(ct);

        if (command.IsPublic)
            await CommunityFeedCacheInvalidator.InvalidateAsync(cacheService, ct);

        return Response<CreateCheckInResponse>.Success(new CreateCheckInResponse(checkIn.Id));
    }
}