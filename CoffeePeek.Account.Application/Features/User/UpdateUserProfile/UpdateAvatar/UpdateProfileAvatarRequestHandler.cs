using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.PhotoMetadataAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAvatar;

public static class UpdateUserAvatarRequestHandler
{
    public static async Task<UpdateEntityResponse<PhotoMetadata>> Handle(
        UpdateUserAvatarCommand request, 
        IUserRepository userRepository,
        IPhotoMetadataRepository photoMetadataRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher publishEndpoint,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct)
                   ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        var oldPhoto = user.PhotoMetadata;

        var photoMetadata = PhotoMetadata.Create(
            request.UploadedPhoto.FileName,
            request.UploadedPhoto.ContentType,
            request.UploadedPhoto.StorageKey,
            request.UploadedPhoto.Size);
        
        photoMetadataRepository.Add(photoMetadata);
        user.UpdateAvatar(photoMetadata);

        if (oldPhoto != null)
        {
            await publishEndpoint.Publish(new PhotoReplacedEvent(
                oldPhoto.Id,
                oldPhoto.StorageKey,
                photoMetadata.Id,
                "User",
                user.Id,
                DateTime.UtcNow), ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return UpdateEntityResponse<PhotoMetadata>.Success(user.PhotoMetadata!, "Photo updated successfully");
    }
}