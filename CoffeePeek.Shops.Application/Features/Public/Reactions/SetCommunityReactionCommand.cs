using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Shops.Application.Features.Public.Reactions;

public record SetCommunityReactionCommand(
    [Required] CommunityCommentTargetType TargetType,
    [Required] Guid TargetId,
    CommunityReactionType? ReactionType)
{
    [JsonIgnore] public Guid UserId { get; init; }
    [JsonIgnore] public string UserName { get; init; } = string.Empty;
}

public record SetCommunityReactionResponse(
    CommunityReactionType? ActiveReaction,
    bool WasRemoved);
