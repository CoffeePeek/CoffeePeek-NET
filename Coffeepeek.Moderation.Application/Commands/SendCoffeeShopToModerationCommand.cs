using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop.Review;
using MediatR;

namespace CoffeePeek.Moderation.Application.Commands;

public class SendCoffeeShopToModerationCommand : IRequest<Response<SendCoffeeShopToModerationResponse>>
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
    public List<PhotoMetadataDto>? ShopPhotos { get; set; }
}