using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Commands;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Repositories;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.MediaService.Handlers;

public static class ConfirmPhotoHandler
{
    public static async Task<(Response<object>, PhotoConfirmedEvent[])> Handle(
        ConfirmPhotoCommand command,
        IPhotoRepository repository,
        CancellationToken ct)
    {
        var photo = await repository.GetByIdAsync(command.PhotoId, ct);
        
        if (photo == null) 
            return (Response<object>.Error("Photo not found"), []);

        if (photo.Status != PhotoStatus.Pending)
            return (Response<object>.Error("Only pending photos can be confirmed"), []);

        photo.Status = PhotoStatus.Confirmed;

        var photoEvent = new PhotoConfirmedEvent(
            photo.Id,
            photo.StorageKey,
            photo.OwnerType.ToString(),
            photo.OwnerId,
            photo.Id,
            DateTime.UtcNow);

        return (Response<object>.Success(null, "Confirmed"), [photoEvent]);
    }
}