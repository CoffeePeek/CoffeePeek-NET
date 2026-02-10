using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class UserPhotoUploadedHandler
{
    [Transactional]
    public async Task Handle(
        PhotoUploadedEvent @event, 
        IUserRepository userRepository, 
        CancellationToken ct)
    {
        var user = await userRepository.GetById(@event.OwnerId, ct);

        if (user == null) return;

        var photo = PhotoMetadata.Create(
            @event.FileName, 
            @event.ContentType, 
            @event.StorageKey, 
            @event.SizeBytes);
            
        user.UpdateAvatar(photo);
    }
}