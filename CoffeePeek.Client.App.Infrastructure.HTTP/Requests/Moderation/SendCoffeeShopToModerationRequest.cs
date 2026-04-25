using CoffeePeek.Client.App.Infrastructure.HTTP.Requests;
using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Dtos.Contact;
using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Requests.Moderation;

public sealed class SendCoffeeShopToModerationRequest : BaseRequest
{
    public required string Name { get; init; }

    public required string Address { get; init; }

    public required Guid CityId { get; init; }

    public string? Description { get; init; }

    public PriceRange? PriceRange { get; init; }

    public ShopContactDto? ShopContact { get; init; }

    public List<ScheduleDto>? Schedules { get; init; }

    public List<Guid>? EquipmentIds { get; init; }

    public List<Guid>? CoffeeBeanIds { get; init; }

    public List<Guid>? RoasterIds { get; init; }

    public List<Guid>? BrewMethodIds { get; init; }

    public List<UploadedPhotoDto>? ShopPhotos { get; init; }
}
