using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace Coffeepeek.Moderation.Application.Commands;

public class SendCoffeeShopToModerationRequest : IRequest<Response<SendCoffeeShopToModerationResponse>>
{
    [Required]
    public string Name { get; set; }
    [Required]
    [JsonPropertyName("fullAddress")]
    public string NotValidatedAddress { get; set; }
    [JsonIgnore]
    public Guid UserId { get; set; }
    
    public string? Description { get; set; }
    public PriceRange? PriceRange { get; set; }
    public Guid? CityId { get; set; }
    public ShopContactDto? ShopContact { get; set; }
    public List<ScheduleDto>? Schedules { get; set; }
    public List<Guid>? EquipmentIds { get; set; }
    public List<Guid>? CoffeeBeanIds { get; set; }
    public List<Guid>? RoasterIds { get; set; }
    public List<Guid>? BrewMethodIds { get; set; }
    public List<byte[]>? ShopPhotos { get; set; }
}