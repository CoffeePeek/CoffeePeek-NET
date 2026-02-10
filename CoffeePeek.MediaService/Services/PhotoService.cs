using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Requests;
using CoffeePeek.MediaService.Responses;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.MediaService.Services;

public class PhotoService(
    IStorageService storageService,
    IUnitOfWork unitOfWork,
    IGenericRepository<PhotoMetadata> repository,
    ICapPublisher capPublisher) : IPhotoService
{
    public async Task<Response<GenerateUploadUrlResponse>> GenerateUserAvatarUploadUrl(UploadUrlRequest request,
        CancellationToken ct)
    {
        if (request.SizeBytes > 5 * 1024 * 1024 ) // > 5MB
        {
             throw new ValidationException("File size should be less than 5MB");
        }
        
        var (url, key) = await storageService.GetPresignedUploadUrl(request.FileName, request.ContentType, BucketType.User, ct);

        var photoMetadata = new PhotoMetadata
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            ContentType = request.ContentType,
            StorageKey = key,
            SizeBytes = request.SizeBytes,
            BucketType = BucketType.User,
            OwnerType = OwnerType.User,
            OwnerId = request.OwnerId,
            Status = PhotoStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await PublishPhotoUploadedEvent(photoMetadata);
            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        var result = new GenerateUploadUrlResponse(photoMetadata.Id, url, key);

        return Response<GenerateUploadUrlResponse>.Success(result);
    }

    public async Task<Response<List<GenerateUploadUrlResponse>>> GenerateShopUploadUrls(List<UploadUrlRequest> requests,
        CancellationToken ct)
    {
        var results = new List<GenerateUploadUrlResponse>();

        var photoMetadataList = new List<PhotoMetadata>();
        
        foreach (var req in requests)
        {
            var (url, key) = await storageService.GetPresignedUploadUrl(req.FileName, req.ContentType, BucketType.Shop, ct);

            var photoMetadata = new PhotoMetadata
            {
                Id = Guid.NewGuid(),
                FileName = req.FileName,
                ContentType = req.ContentType,
                StorageKey = key,
                SizeBytes = req.SizeBytes,
                BucketType = BucketType.Shop,
                OwnerType = OwnerType.Shop,
                OwnerId = req.OwnerId,
                Status = PhotoStatus.Pending,
                UploadedAt = DateTime.UtcNow
            };

            photoMetadataList.Add(photoMetadata);
            repository.Add(photoMetadata);
            results.Add(new GenerateUploadUrlResponse(photoMetadata.Id, url, key));
        }

        await unitOfWork.SaveChangesAsync(ct);

        foreach (var photoMetadata in photoMetadataList)
        {
            await PublishPhotoUploadedEvent(photoMetadata);
        }

        return Response<List<GenerateUploadUrlResponse>>.Success(results);
    }

    public async Task<Response<object>> ConfirmPhoto(Guid photoId, CancellationToken ct)
    {
        var photo = await repository.GetByIdAsync(photoId, ct);
        if (photo == null)
            return Response<object>.Error("Photo not found");

        if (photo.Status != PhotoStatus.Pending)
            return Response<object>.Error("Only pending photos can be confirmed");

        photo.Status = PhotoStatus.Confirmed;
        await unitOfWork.SaveChangesAsync(ct);

        await capPublisher.PublishAsync(CapEventNames.Media.PhotoConfirmed, new PhotoConfirmedEvent(
            photo.Id,
            photo.StorageKey,
            photo.OwnerType.ToString(),
            photo.OwnerId,
            photo.Id,
            DateTime.UtcNow), cancellationToken: ct);

        return Response<object>.Success(null, "Photo confirmed successfully");
    }

    public async Task<Response<object>> DeletePhoto(Guid photoId, CancellationToken ct)
    {
        var photo = await repository.GetByIdAsync(photoId, ct);
        if (photo == null)
        {
            return Response<object>.Error("Photo not found");
        }

        await storageService.Delete(photo.StorageKey, (BucketType)(int)photo.BucketType, ct);

        photo.Status = PhotoStatus.Deleted;
        await unitOfWork.SaveChangesAsync(ct);

        await capPublisher.PublishAsync(CapEventNames.Media.PhotoDeleted, new PhotoDeletedEvent(
            photo.Id,
            photo.StorageKey,
            photo.OwnerType.ToString(),
            photo.OwnerId,
            DateTime.UtcNow), cancellationToken: ct);

        return Response<object>.Success(null, "Photo deleted successfully");
    }

    private async Task PublishPhotoUploadedEvent(PhotoMetadata photo)
    {
        await capPublisher.PublishAsync(CapEventNames.Media.PhotoUploaded, new PhotoUploadedEvent(
            photo.Id,
            photo.StorageKey,
            photo.FileName,
            photo.ContentType,
            photo.SizeBytes,
            photo.OwnerType.ToString(),
            photo.OwnerId,
            photo.UploadedAt));
    }
}
