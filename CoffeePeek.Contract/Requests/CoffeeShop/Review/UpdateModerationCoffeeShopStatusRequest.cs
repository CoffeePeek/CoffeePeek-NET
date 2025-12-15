using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public record UpdateModerationCoffeeShopStatusRequest(Guid Id, ModerationStatus ModerationStatus, Guid UserId)
    : IRequest<Responses.Response>
{
    [JsonIgnore] public Guid UserId { get; } = UserId;
    public Guid Id { get; } = Id;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ModerationStatus ModerationStatus { get; } = ModerationStatus;
}