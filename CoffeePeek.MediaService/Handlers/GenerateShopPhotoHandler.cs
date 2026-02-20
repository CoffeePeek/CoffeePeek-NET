using CoffeePeek.Contract.Events;
using CoffeePeek.MediaService.Commands;
using CoffeePeek.MediaService.Configuration;
using CoffeePeek.MediaService.Data;
using CoffeePeek.MediaService.Factories;
using CoffeePeek.MediaService.Repositories;
using CoffeePeek.MediaService.Responses;
using CoffeePeek.MediaService.Services;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.MediaService.Handlers;

public class GenerateShopPhotoHandler
{
    public static async Task<Response<List<GenerateUploadUrlResponse>>> Handle(
        GenerateShopPhotosCommand command,
        IStorageService storageService,
        IPhotoRepository repository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        CancellationToken ct)
    {
        var metadataList = new List<PhotoMetadata>();
        var uploadUrls = new Dictionary<Guid, string>();

        foreach (var request in command.Requests)
        {
            var presigned = await storageService
                .GetPresignedUploadUrl(request.FileName, request.ContentType, BucketType.Shop, ct);

            var metadata = PhotoMetadataFactory.Create(
                request.FileName, request.ContentType, presigned.StorageKey,
                request.SizeBytes, BucketType.Shop, OwnerType.Shop, command.OwnerId);

            metadataList.Add(metadata);
            uploadUrls[metadata.Id] = presigned.Url;
        }

        repository.AddRange(metadataList);

        await eventPublisher.Publish(
            new PhotosUploadedEvent(metadataList.Select(e => new PhotoUploadedEvent(
                e.Id,
                e.StorageKey,
                e.FileName,
                e.ContentType,
                e.SizeBytes,
                e.OwnerType.ToString(),
                e.OwnerId,
                DateTime.UtcNow))), ct);

        await unitOfWork.SaveChangesAsync(ct);

        var results =  metadataList
            .Select(m => new GenerateUploadUrlResponse(m.Id, uploadUrls[m.Id], m.StorageKey))
            .ToList();
        
        return Response<List<GenerateUploadUrlResponse>>.Success(results);
    }
}