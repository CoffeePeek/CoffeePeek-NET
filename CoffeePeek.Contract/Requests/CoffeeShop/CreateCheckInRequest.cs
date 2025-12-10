using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record CreateCheckInRequest : IRequest<Response<CreateCheckInResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; init; }
    
    [Required]
    public Guid ShopId { get; init; }
    
    [MaxLength(500)]
    public string? Note { get; init; }
    
    public CheckInReviewRequest? Review { get; init; }
}