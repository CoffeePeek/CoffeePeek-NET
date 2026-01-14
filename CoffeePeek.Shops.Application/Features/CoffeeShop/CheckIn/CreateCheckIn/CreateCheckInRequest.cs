using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shops.Domain;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn.CreateCheckIn;

public record CreateCheckInRequest(
    [Required] Guid ShopId,
    [MaxLength(BusinessConstants.MaxCheckInNoteLength)]
    string Note,
    CheckInReviewCommand? Review) : IRequest<Response<CreateCheckInResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; init; }
}