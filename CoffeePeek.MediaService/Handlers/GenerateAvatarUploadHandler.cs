using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Commands;
using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Repositories;
using CoffeePeek.MediaService.Responses;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.MediaService.Handlers;

public static class GenerateAvatarUploadHandler
{
    public static async Task<Response<GenerateUploadUrlResponse>> Handle(
        GenerateAvatarUploadCommand command,
        IStorageService storageService,
        IPhotoRepository repository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        CancellationToken ct)
    {
        if (command.SizeBytes > 5 * 1024 * 1024)
            throw new ValidationException("File size should be less than 5MB");

        var (url, key) = await storageService.GetPresignedUploadUrl(
            command.FileName, command.ContentType, BucketType.User, ct);

        var metadata = new PhotoMetadata
        {
            Id = Guid.NewGuid(),
            FileName = command.FileName,
            ContentType = command.ContentType,
            StorageKey = key,
            SizeBytes = command.SizeBytes,
            BucketType = BucketType.User,
            OwnerType = OwnerType.User,
            OwnerId = command.OwnerId,
            Status = PhotoStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };

        repository.Add(metadata);

        await eventPublisher.Publish(new PhotoUploadedEvent(
            metadata.Id, 
            metadata.StorageKey, 
            metadata.FileName, 
            metadata.ContentType,
            metadata.SizeBytes, 
            metadata.OwnerType.ToString(), 
            metadata.OwnerId, 
            DateTime.UtcNow), ct);

        await unitOfWork.SaveChangesAsync(ct);

        return Response<GenerateUploadUrlResponse>.Success(new GenerateUploadUrlResponse(metadata.Id, url, key));
    }
}