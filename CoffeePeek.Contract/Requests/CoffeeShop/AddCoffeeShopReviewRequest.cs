using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response.CoffeeShop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class AddCoffeeShopReviewRequest : IRequest<Response.Response<AddCoffeeShopReviewResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    
    public int ShopId { get; init; }
    
    public string Header { get; init; }
    public string Comment { get; init; }

    public int RatingCoffee { get; init; }
    public int RatingService { get; init; }
    public int RatingPlace { get; init; }
}