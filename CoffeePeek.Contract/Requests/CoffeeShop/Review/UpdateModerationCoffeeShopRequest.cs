using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public class UpdateModerationCoffeeShopRequest : IRequest<Response<UpdateModerationCoffeeShopResponse>>
{
    [JsonIgnore] public Guid UserId { get; set; }
    public int ReviewShopId { get; set; }
    public string? Description { get; set; }
    public List<byte[]>? ShopPhotos { get; set; }
    public List<ScheduleDto>? Schedules { get; set; }
}