using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Responses;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class UpdateCoffeeShopReviewRequest : IRequest<Response<UpdateCoffeeShopReviewResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    
    public Guid ReviewId { get; set; }
    
    public string Header { get; set; }
    public string Comment { get; set; }
    
    [Range(1, 5, ErrorMessage = "RatingCoffee must be between 1 and 5")]
    public int RatingCoffee { get; set; }
    
    [Range(1, 5, ErrorMessage = "RatingService must be between 1 and 5")]
    public int RatingService { get; set; }
    
    [Range(1, 5, ErrorMessage = "RatingPlace must be between 1 and 5")]
    public int RatingPlace { get; set; }
}