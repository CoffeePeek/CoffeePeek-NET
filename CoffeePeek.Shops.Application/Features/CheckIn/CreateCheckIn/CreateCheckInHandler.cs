using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Events.Shops;
using CoffeePeek.Shared.Extensions.CAP;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Validation;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Entities;
using CoffeePeek.Shops.Domain.Entities.UserFavoriteAggregate;
using DotNetCore.CAP;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;

using Review = Domain.Entities.ReviewAggregate.Review;

public class CreateCheckInHandler(
    IGenericRepository<Domain.Aggregates.CheckInAggregate.CheckIn> checkInRepository,
    IAsyncValidationStrategy<CreateCheckInCommand> validationStrategy,
    IUnitOfWork unitOfWork,
    ICapPublisher capPublisher,
    IMapper mapper)
    : IRequestHandler<CreateCheckInCommand, Response<CreateCheckInResponse>>
{
    public async Task<Response<CreateCheckInResponse>> Handle(CreateCheckInCommand command, CancellationToken ct)
    {
        var validationResult = await validationStrategy.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.ErrorMessage!);
        }

        var checkIn = Domain.Aggregates.CheckInAggregate.CheckIn.Create(command.UserId, command.CoffeeShopId, command.VisitedAt);

        if (!string.IsNullOrEmpty(command.Note))
        {
            checkIn.UpdateNote(command.Note);    
        }

        if (command.Photos != null && command.Photos.Count != 0)
        {
            var photos = command.Photos.Select(x =>
                new ShopPhoto(x.FileName, x.ContentType, x.StorageKey, x.Size, command.UserId));
            checkIn.AddPhotos(photos);
        }

        using var trans = unitOfWork.BeginTransactionAsync(ct);

        if (command.IsPublic)
        {
            try
            {
                var commentPreview = string.Join(" ",
                    command.Note?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(3) ?? []);

                var review = Review.Create(command.CoffeeShopId,
                    command.UserId, command.UserName,
                    header: commentPreview,
                    comment: command.Note!,
                    ratingDto: command.Rating!);

                await capPublisher.PublishAsync(new CheckinCreatedEvent
                {
                    UserId = command.UserId,
                    ShopId = command.CoffeeShopId,
                    CreatedAt = checkIn.CreatedAtUtc,
                    ReviewDto = mapper.Map<ReviewDto>(review)
                }, cancellationToken: ct);
            }
            catch (DomainException ex)
            {
                //ignore 
            }

            checkInRepository.Add(checkIn);

            await unitOfWork.SaveChangesAsync(ct);
        }

        await unitOfWork.CommitTransactionAsync(ct);

        return Response<CreateCheckInResponse>.Success(new CreateCheckInResponse(checkIn.Id));
    }
}