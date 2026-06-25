using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Events.Community;

public record CommunityReactionNotificationEvent(
    Guid RecipientUserId,
    Guid ActorUserId,
    string ActorUserName,
    CommunityCommentTargetType TargetType,
    Guid TargetId,
    CommunityReactionType ReactionType);
