using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class UpdateCoffeeShopReviewRequest : IRequest<Response.Response<UpdateCoffeeShopReviewResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    
    public Guid ReviewId { get; set; }
    
    public string Header { get; set; }
    public string Comment { get; set; }
    
    public int RatingCoffee { get; set; }
    public int RatingService { get; set; }
    public int RatingPlace { get; set; }
}