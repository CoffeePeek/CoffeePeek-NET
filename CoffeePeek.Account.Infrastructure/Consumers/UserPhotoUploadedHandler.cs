using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Kernel;

namespace CoffeePeek.Account.Infrastructure.Consumers;

public class UserPhotoUploadedHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
{
    public async Task Handle(PhotoUploadedEvent message)
    {
        var user = await userRepository.GetById(message.OwnerId);

        if (user == null) return;

        var photo = PhotoMetadata.Create(
            message.FileName, 
            message.ContentType, 
            message.StorageKey, 
            message.SizeBytes);
            
        user.UpdateAvatar(photo);

        await unitOfWork.SaveChangesAsync();
    }
}