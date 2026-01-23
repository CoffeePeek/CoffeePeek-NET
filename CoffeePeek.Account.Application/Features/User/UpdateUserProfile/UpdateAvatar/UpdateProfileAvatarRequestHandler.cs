using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Exceptions;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Abstract.S3;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAvatar;

public class UpdateUserAvatarRequestHandler(
    IUserRepository userRepository,
    IGenericRepository<PhotoMetadata> photoMetadataRepository,
    IUnitOfWork unitOfWork,
    IStorageService storageService) : IRequestHandler<UpdateUserAvatarCommand, UpdateEntityResponse<PhotoMetadata>>
{
    public async Task<UpdateEntityResponse<PhotoMetadata>> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UploadedPhoto.FileName))
            throw new ValidationException("File name cannot be empty");

        var user = await userRepository.GetById(request.UserId, cancellationToken) 
                   ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        var fileExists = await storageService.ExistsAsync(request.UploadedPhoto.StorageKey);
        if (!fileExists)
            throw new ValidationException("File was not uploaded to storage or session expired");

        var oldStorageKey = user.PhotoMetadata?.StorageKey;

        var photoMetadata = PhotoMetadata.Create(request.UploadedPhoto.FileName,
            request.UploadedPhoto.ContentType,
            request.UploadedPhoto.StorageKey,
            request.UploadedPhoto.Size);
        
        photoMetadataRepository.Add(photoMetadata);
        user.UpdateAvatar(photoMetadata);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await storageService.MarkAsPermanentAsync(request.UploadedPhoto.StorageKey);

        if (!string.IsNullOrEmpty(oldStorageKey) && oldStorageKey != request.UploadedPhoto.StorageKey)
        {
            await storageService.DeleteAsync(oldStorageKey);
        }

        return UpdateEntityResponse<PhotoMetadata>.Success(user.PhotoMetadata!, "Photo updated successfully");
    }
}