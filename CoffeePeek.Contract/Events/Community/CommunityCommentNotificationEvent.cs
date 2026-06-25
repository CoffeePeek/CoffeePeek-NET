using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Events.Community;

public record CommunityCommentNotificationEvent(
    Guid RecipientUserId,
    Guid ActorUserId,
    string ActorUserName,
    CommunityCommentTargetType TargetType,
    Guid TargetId,
    Guid CommentId);
