using CoffeePeek.Contract.Dtos;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Profile;

public sealed class UpdateAvatarRequest : BaseRequest
{
    public required UploadedPhotoDto UploadedPhoto { get; init; }
}
