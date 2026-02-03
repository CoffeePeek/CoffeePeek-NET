using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Requests;
using CoffeePeek.MediaService.Responses;
using CoffeePeek.Shared.Infrastructure.Constants;
using DotNetCore.CAP;

namespace CoffeePeek.MediaService.Services;

public class PhotoService(
    IStorageService storageService,
    MediaDbContext dbContext,
    ICapPublisher capPublisher) : IPhotoService
{
    public async Task<Response<GenerateUploadUrlResponse>> GenerateUserAvatarUploadUrl(UploadUrlRequest request,
        CancellationToken ct)
    {
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
        
        await using (var transaction = await dbContext.Database.BeginTransactionAsync(ct))
        {
            dbContext.Photos.Add(photoMetadata);
            await PublishPhotoUploadedEvent(photoMetadata);
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        

        var result = new GenerateUploadUrlResponse(photoMetadata.Id, url, key);

        return Response<GenerateUploadUrlResponse>.Success(result);
    }

    public async Task<Response<List<GenerateUploadUrlResponse>>> GenerateShopUploadUrls(List<UploadUrlRequest> requests,
        CancellationToken ct)
    {
        var results = new List<GenerateUploadUrlResponse>();

        foreach (var req in requests)
        {
            var (url, key) = await storageService.GetPresignedUploadUrl(req.FileName, req.ContentType, Configuration.BucketType.Shop);

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

            dbContext.Photos.Add(photoMetadata);
            results.Add(new GenerateUploadUrlResponse(photoMetadata.Id, url, key));
        }

        await dbContext.SaveChangesAsync(ct);

        foreach (var result in results)
        {
            var photo = await dbContext.Photos.FindAsync([result.PhotoId], ct);
            if (photo != null)
            {
                await PublishPhotoUploadedEvent(photo);
            }
        }

        return Response<List<GenerateUploadUrlResponse>>.Success(results);
    }

    public async Task<Response<object>> ConfirmPhoto(Guid photoId, CancellationToken ct)
    {
        var photo = await dbContext.Photos.FindAsync([photoId], ct);
        if (photo == null)
            return Response<object>.Error("Photo not found");

        if (photo.Status != PhotoStatus.Pending)
            return Response<object>.Error("Only pending photos can be confirmed");

        photo.Status = PhotoStatus.Confirmed;
        await dbContext.SaveChangesAsync(ct);

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
        var photo = await dbContext.Photos.FindAsync([photoId], ct);
        if (photo == null)
        {
            return Response<object>.Error("Photo not found");
        }

        await storageService.Delete(photo.StorageKey, (Configuration.BucketType)(int)photo.BucketType);

        photo.Status = PhotoStatus.Deleted;
        await dbContext.SaveChangesAsync(ct);

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
