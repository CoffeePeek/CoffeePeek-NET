using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAvatar;

public record UpdateUserAvatarCommand([property: JsonIgnore] Guid UserId, UploadedPhotoDto UploadedPhoto);
