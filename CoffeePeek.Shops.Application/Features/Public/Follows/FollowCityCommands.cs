using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeePeek.Shops.Application.Features.Public.Follows;

public record FollowCityCommand([Required] Guid CityId)
{
    [JsonIgnore] public Guid UserId { get; init; }
}

public record UnfollowCityCommand([Required] Guid CityId)
{
    [JsonIgnore] public Guid UserId { get; init; }
}

public record GetFollowedCitiesQuery
{
    [JsonIgnore] public Guid UserId { get; init; }
}

public record GetFollowedCitiesResponse(IReadOnlyList<Guid> CityIds);
