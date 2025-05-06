using System.Text.Json.Serialization;
using CoffeePeek.Domain.Enums.Shop;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public class UpdateReviewCoffeeShopStatusRequest(int id, ReviewStatus reviewStatus, int userId) 
    : IRequest<Response.Response>
{
    public int UserId { get; set; } = userId;
    public int Id { get; set; } = id;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReviewStatus ReviewStatus { get; set; } = reviewStatus;
}