using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.Shops.Domain;
using MediatR;

namespace CoffeePeek.Shops.Application.Features.CoffeeShop.CheckIn.CreateCheckIn;

public record CreateCheckInCommand(
    [property: JsonIgnore] Guid UserId,
    [property: JsonIgnore] string UserName,
    [Required] Guid ShopId,
    [MaxLength(BusinessConstants.MaxCheckInNoteLength)]
    string Note,
    CheckInReviewCommand? Review) : IRequest<Response<CreateCheckInResponse>>;
