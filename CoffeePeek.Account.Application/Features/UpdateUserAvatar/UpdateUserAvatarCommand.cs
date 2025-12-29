using System.Text.Json.Serialization;
using CoffeePeek.Account.Domain.Aggregates;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.UpdateUserAvatar;

public record UpdateUserAvatarCommand(Guid UserId, UploadedPhotoDto? UploadedPhoto)
    : IRequest<UpdateEntityResponse<PhotoMetadata>>
{
    [JsonIgnore] public Guid UserId { get; set; } = UserId;
    public UploadedPhotoDto? UploadedPhoto { get; set; } = UploadedPhoto;
}