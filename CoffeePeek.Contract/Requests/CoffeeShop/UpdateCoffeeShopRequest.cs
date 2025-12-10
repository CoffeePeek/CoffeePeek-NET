using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop;

public class UpdateCoffeeShopRequest : IRequest<Response<UpdateCoffeeShopResponse>>
{
    [JsonIgnore] public int UserId { get; set; }
    public int ShopId { get; set; }
    public string? Description { get; set; }
    public List<byte[]>? ShopPhotos { get; set; }
    public List<ScheduleDto>? Schedules { get; set; }
}