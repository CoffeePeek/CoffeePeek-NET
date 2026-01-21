using System.Text.Json.Serialization;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Shop.UpdateModerationShopStatus;

public record UpdateModerationCoffeeShopStatusCommand(Guid UserId, Guid Id,
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    ModerationStatus ModerationStatus)
    : IRequest<Response>;