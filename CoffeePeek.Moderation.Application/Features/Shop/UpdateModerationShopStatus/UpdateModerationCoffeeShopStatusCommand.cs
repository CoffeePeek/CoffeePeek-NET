using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;

public record UpdateModerationCoffeeShopStatusCommand(Guid UserId, Guid Id,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] ModerationStatus ModerationStatus);