using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Shops.Domain;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CheckIn.CreateCheckIn;

public record CreateCheckInCommand(
    [Required] Guid CoffeeShopId,
    [Required] bool IsPublic,
    [Required] DateTime VisitedAt,
    [MaxLength(BusinessConstants.MaxCheckInNoteLength)]
    string? Note,
    ICollection<UploadedPhotoDto>? Photos,
    RatingDto? Rating)
    : IRequest<Response<CreateCheckInResponse>>
{
    [JsonIgnore] public Guid UserId { get; set; }
    [JsonIgnore] public string UserName { get; set; } = string.Empty;
}