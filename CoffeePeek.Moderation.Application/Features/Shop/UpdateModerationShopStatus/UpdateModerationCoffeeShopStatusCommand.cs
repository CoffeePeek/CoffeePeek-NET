using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;

public record UpdateModerationCoffeeShopStatusCommand(
    [property: JsonIgnore] Guid UserId,
    Guid Id,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] ModerationStatus ModerationStatus,
    [property: MaxLength(BusinessConstants.MaxRejectReasonCommentLength)] string? Comment = null);
