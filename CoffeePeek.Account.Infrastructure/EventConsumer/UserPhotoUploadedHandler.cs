using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;

namespace CoffeePeek.Account.Infrastructure.EventConsumer;

public class UserPhotoUploadedHandler(IUserRepository userRepository, IUnitOfWork unitOfWork) : ICapSubscribe
{
    [CapSubscribe(CapEventNames.Media.PhotoUploaded)]
    public async Task Handle(PhotoUploadedEvent @event, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(@event.OwnerId, cancellationToken);

        if (user == null) return;

        var photo = PhotoMetadata.Create(@event.FileName, @event.ContentType, @event.StorageKey, @event.SizeBytes);
        user.UpdateAvatar(photo);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}