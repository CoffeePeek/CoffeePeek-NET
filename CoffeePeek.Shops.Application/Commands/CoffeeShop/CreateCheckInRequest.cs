using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shops.Domain;
using MediatR;

namespace CoffeePeek.Shops.Application.Commands.CoffeeShop;

public record CreateCheckInRequest(
    [Required] Guid ShopId,
    [MaxLength(BusinessConstants.MaxCheckInNoteLength)]
    string Note,
    CheckInReviewCommand Review) : IRequest<Response<CreateCheckInResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; init; }
}