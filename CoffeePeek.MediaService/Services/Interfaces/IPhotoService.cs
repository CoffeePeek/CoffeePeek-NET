using CoffeePeek.Contract.Abstract;
using CoffeePeek.MediaService.Requests;
using CoffeePeek.MediaService.Responses;

namespace CoffeePeek.MediaService.Services;

public interface IPhotoService
{
    Task<Response<GenerateUploadUrlResponse>> GenerateUserAvatarUploadUrl(UploadUrlRequest request, CancellationToken ct);
    Task<Response<List<GenerateUploadUrlResponse>>> GenerateShopUploadUrls(List<UploadUrlRequest> requests, CancellationToken ct);
    Task<Response<object>> ConfirmPhoto(Guid photoId, CancellationToken ct);
    Task<Response<object>> DeletePhoto(Guid photoId, CancellationToken ct);
}