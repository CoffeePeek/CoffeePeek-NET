using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public record AddCoffeeShopReviewRequest : IRequest<Response.Response<AddCoffeeShopReviewResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    
    public Guid ShopId { get; init; }
    
    public string Header { get; init; }
    public string Comment { get; init; }
    
    [Range(1, 5, ErrorMessage = "RatingCoffee must be between 1 and 5")]
    public int RatingCoffee { get; init; }
    
    [Range(1, 5, ErrorMessage = "RatingService must be between 1 and 5")]
    public int RatingService { get; init; }
    
    [Range(1, 5, ErrorMessage = "RatingPlace must be between 1 and 5")]
    public int RatingPlace { get; init; }
}