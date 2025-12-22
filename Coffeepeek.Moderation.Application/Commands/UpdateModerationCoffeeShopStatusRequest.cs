using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Moderation.Application.Commands;

public record UpdateModerationCoffeeShopStatusRequest(Guid Id, ModerationStatus ModerationStatus, Guid UserId)
    : IRequest<Response>
{
    [JsonIgnore] public Guid UserId { get; } = UserId;
    public Guid Id { get; } = Id;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ModerationStatus ModerationStatus { get; } = ModerationStatus;
}