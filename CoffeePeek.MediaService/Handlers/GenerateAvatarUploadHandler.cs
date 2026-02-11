using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Commands;
using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Repositories;
using CoffeePeek.MediaService.Responses;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.MediaService.Handlers;

public static class GenerateAvatarUploadHandler
{
    [Transactional]
    public static async Task<(Response<GenerateUploadUrlResponse>, PhotoUploadedEvent)> Handle(
        GenerateAvatarUploadCommand command,
        IStorageService storageService,
        IPhotoRepository repository,
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

        var response = new GenerateUploadUrlResponse(metadata.Id, url, key);
        var @event = new PhotoUploadedEvent(metadata.Id, metadata.StorageKey, metadata.FileName, metadata.ContentType,
            metadata.SizeBytes, metadata.OwnerType.ToString(), metadata.OwnerId, DateTime.UtcNow);

        return (Response<GenerateUploadUrlResponse>.Success(response), @event);
    }
}