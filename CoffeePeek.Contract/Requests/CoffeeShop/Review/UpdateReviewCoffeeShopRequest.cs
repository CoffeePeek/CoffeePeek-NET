using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.Address;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public class UpdateReviewCoffeeShopRequest : IRequest<Response<UpdateReviewCoffeeShopResponse>>
{
    [JsonIgnore] public int UserId { get; set; }
    public int ReviewShopId { get; set; }
    public string? Description { get; set; }
    public AddressDto? Address { get; set; }
    public List<byte[]>? ShopPhotos { get; set; }
    public List<ScheduleDto>? Schedules { get; set; }
}