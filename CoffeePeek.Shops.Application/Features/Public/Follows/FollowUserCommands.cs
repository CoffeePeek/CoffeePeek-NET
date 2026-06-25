using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeePeek.Shops.Application.Features.Public.Follows;

public record FollowUserCommand([Required] Guid FollowingUserId)
{
    [JsonIgnore] public Guid FollowerId { get; init; }
    [JsonIgnore] public string FollowerUserName { get; init; } = string.Empty;
}

public record UnfollowUserCommand([Required] Guid FollowingUserId)
{
    [JsonIgnore] public Guid FollowerId { get; init; }
}

public record GetFollowingQuery
{
    [JsonIgnore] public Guid UserId { get; init; }
}

public record GetFollowingResponse(IReadOnlyList<Guid> FollowingUserIds);
