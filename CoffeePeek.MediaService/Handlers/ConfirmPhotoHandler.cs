using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Repositories;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.MediaService.Handlers;

public record ConfirmPhotoCommand(Guid PhotoId);

public static class ConfirmPhotoHandler
{
    [Transactional]
    public static async Task<(Response<object>, PhotoConfirmedEvent?)> Handle(
        ConfirmPhotoCommand command,
        IPhotoRepository repository,
        CancellationToken ct)
    {
        var photo = await repository.GetByIdAsync(command.PhotoId, ct);
        if (photo == null) 
            return (Response<object>.Error("Photo not found"), null);

        if (photo.Status != PhotoStatus.Pending)
            return (Response<object>.Error("Only pending photos can be confirmed"), null);

        photo.Status = PhotoStatus.Confirmed;

        var @event = new PhotoConfirmedEvent(
            photo.Id,
            photo.StorageKey,
            photo.OwnerType.ToString(),
            photo.OwnerId,
            photo.Id,
            DateTime.UtcNow);

        return (Response<object>.Success(null, "Confirmed"), @event);
    }
}