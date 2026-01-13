using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shops.Domain;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn;

public record CreateCheckInRequest(
    [Required] Guid ShopId,
    [MaxLength(BusinessConstants.MaxCheckInNoteLength)]
    string Note,
    CheckInReviewCommand? Review) : IRequest<Response<CreateCheckInResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; init; }
}