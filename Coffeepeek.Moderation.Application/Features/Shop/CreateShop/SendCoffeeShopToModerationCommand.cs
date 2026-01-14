using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using FluentResults;
using MediatR;

namespace Coffeepeek.Moderation.Application.Features.Shop.CreateShop;

public record SendCoffeeShopToModerationCommand : IRequest<Response<SendCoffeeShopToModerationResponse>>
{
    [Required, MaxLength(55)]
    public string Name { get; set; }
    [Required]
    public string NotValidatedAddress { get; set; }
    [JsonIgnore]
    public Guid UserId { get; set; }
    
    public string? Description { get; set; }
    public PriceRange PriceRange { get; set; }
    [Required] public Guid CityId { get; set; }
    public ShopContactDto? ShopContact { get; set; }
    public List<ScheduleDto>? Schedules { get; set; }
    public List<Guid>? EquipmentIds { get; set; }
    public List<Guid>? CoffeeBeanIds { get; set; }
    public List<Guid>? RoasterIds { get; set; }
    public List<Guid>? BrewMethodIds { get; set; }
    public List<UploadedPhotoDto>? ShopPhotos { get; set; }
}