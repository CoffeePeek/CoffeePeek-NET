using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class CreateCheckInRequest : IRequest<Response<CreateCheckInResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid ShopId { get; init; }
    
    [MaxLength(500)]
    public string? Note { get; init; }
    
    public CheckInReviewRequest? Review { get; init; }
}

public class CheckInReviewRequest
{
    [Required]
    [MaxLength(70)]
    public string Header { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Comment { get; init; } = string.Empty;
    
    [Range(1, 5)]
    public int RatingCoffee { get; init; }
    
    [Range(1, 5)]
    public int RatingPlace { get; init; }
    
    [Range(1, 5)]
    public int RatingService { get; init; }
}