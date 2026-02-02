using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAvatar;

public class UpdateUserAvatarRequestHandler(
    IUserRepository userRepository,
    IGenericRepository<PhotoMetadata> photoMetadataRepository,
    IUnitOfWork unitOfWork,
    ICapPublisher capPublisher) : IRequestHandler<UpdateUserAvatarCommand, UpdateEntityResponse<PhotoMetadata>>
{
    public async Task<UpdateEntityResponse<PhotoMetadata>> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UploadedPhoto.FileName))
            throw new ValidationException("File name cannot be empty");

        var user = await userRepository.GetById(request.UserId, cancellationToken)
                   ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        var oldPhotoId = user.PhotoMetadataId;
        var oldStorageKey = user.PhotoMetadata?.StorageKey;

        var photoMetadata = PhotoMetadata.Create(request.UploadedPhoto.FileName,
            request.UploadedPhoto.ContentType,
            request.UploadedPhoto.StorageKey,
            request.UploadedPhoto.Size);

        photoMetadataRepository.Add(photoMetadata);
        user.UpdateAvatar(photoMetadata);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (oldPhotoId.HasValue && !string.IsNullOrEmpty(oldStorageKey))
        {
            await capPublisher.PublishAsync(CapEventNames.Media.PhotoReplaced, new PhotoReplacedEvent(
                oldPhotoId.Value,
                oldStorageKey,
                photoMetadata.Id,
                "User",
                user.Id,
                DateTime.UtcNow), cancellationToken: cancellationToken);
        }

        return UpdateEntityResponse<PhotoMetadata>.Success(user.PhotoMetadata!, "Photo updated successfully");
    }
}