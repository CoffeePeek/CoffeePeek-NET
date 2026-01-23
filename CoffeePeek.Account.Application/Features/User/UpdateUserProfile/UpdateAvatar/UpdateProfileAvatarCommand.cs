using System.Text.Json.Serialization;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserProfile.UpdateAvatar;

public record UpdateUserAvatarCommand([property: JsonIgnore] Guid UserId, UploadedPhotoDto UploadedPhoto)
    : IRequest<UpdateEntityResponse<PhotoMetadata>>;