using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public class SendCoffeeShopToReviewRequest : IRequest<Response<SendCoffeeShopToReviewResponse>>
{
    [Required]
    public string Name { get; set; }
    [Required]
    [JsonPropertyName("fullAddress")]
    public string NotValidatedAddress { get; set; }
    [JsonIgnore]
    public int UserId { get; set; }
}