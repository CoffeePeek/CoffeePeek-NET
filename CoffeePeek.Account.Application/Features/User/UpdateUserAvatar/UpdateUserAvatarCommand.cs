using System.Text.Json.Serialization;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateUserAvatar;

public record UpdateUserAvatarCommand(Guid UserId, UploadedPhotoDto UploadedPhoto)
    : IRequest<UpdateEntityResponse<PhotoMetadata>>
{
    [JsonIgnore] public Guid UserId { get; set; } = UserId;
    public UploadedPhotoDto UploadedPhoto { get; set; } = UploadedPhoto;
}