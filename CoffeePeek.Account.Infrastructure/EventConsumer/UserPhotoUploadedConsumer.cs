using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Kernel;
using MassTransit;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class UserPhotoUploadedConsumer(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IConsumer<PhotoUploadedEvent>
{
    public async Task Consume(ConsumeContext<PhotoUploadedEvent> context)
    {
        var @event = context.Message;
        var ct = context.CancellationToken;

        var user = await userRepository.GetById(@event.OwnerId, ct);

        if (user == null) return;

        var photo = PhotoMetadata.Create(
            @event.FileName, 
            @event.ContentType, 
            @event.StorageKey, 
            @event.SizeBytes);
            
        user.UpdateAvatar(photo);

        await unitOfWork.SaveChangesAsync(ct);
    }
}