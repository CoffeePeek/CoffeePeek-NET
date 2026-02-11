using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Entities;
using MapsterMapper;
using Wolverine.Attributes;

namespace CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;

using Review = Domain.Aggregates.ReviewAggregate.Review;

public static class CreateCheckInHandler
{
    [Transactional]
    public static async Task<(Response<CreateCheckInResponse>, CheckinCreatedEvent?)> Handle(
        CreateCheckInCommand command,
        IQueryCheckInRepository queryCheckInRepository,
        IAsyncValidationStrategy<CreateCheckInCommand> validationStrategy,
        IMapper mapper,
        CancellationToken ct)
    {
        var validationResult = await validationStrategy.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.ErrorMessage!);
        }

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

        CheckinCreatedEvent? @event = null;

        if (command.IsPublic)
        {
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
                    ratingPlace: command.Rating!.Place, ratingService: command.Rating.Service, ratingCoffee: command.Rating.Coffee);

                @event = new CheckinCreatedEvent
                {
                    UserId = command.UserId,
                    ShopId = command.CoffeeShopId,
                    CreatedAt = checkIn.CreatedAtUtc,
                    ReviewDto = mapper.Map<ReviewDto>(review)
                };
            }
            catch (DomainException) { /* ignore */ }
            
            // Сохраняем чекин только если он публичный (согласно твоей логике)
            queryCheckInRepository.Add(checkIn);
        }

        // 4. Возвращаем кортеж: (Ответ API, Событие для Outbox)
        // Wolverine сам поймет, что Response нужно отдать в контроллер, 
        // а CheckinCreatedEvent (если он не null) отправить в очередь.
        return (Response<CreateCheckInResponse>.Success(new CreateCheckInResponse(checkIn.Id)), @event);
    }
}