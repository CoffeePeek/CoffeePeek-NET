using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.CoffeeShop;

namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class GetAllModerationReviewsResultDto
{
    [JsonPropertyName("reviewDtos")]
    public ModerationReviewDto[] Reviews { get; init; } = [];
}
