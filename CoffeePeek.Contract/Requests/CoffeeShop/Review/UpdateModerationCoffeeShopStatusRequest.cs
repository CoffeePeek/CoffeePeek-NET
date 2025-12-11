using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;
using MediatR;

namespace CoffeePeek.Contract.Requests.CoffeeShop.Review;

public class UpdateModerationCoffeeShopStatusRequest(Guid id, ModerationStatus moderationStatus, Guid userId) 
    : IRequest<Response.Response>
{
    public Guid UserId { get; set; } = userId;
    public Guid Id { get; set; } = id;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ModerationStatus ModerationStatus { get; set; } = moderationStatus;
}