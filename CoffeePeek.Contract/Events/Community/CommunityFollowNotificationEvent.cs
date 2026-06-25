namespace CoffeePeek.Contract.Events.Community;

public record CommunityFollowNotificationEvent(
    Guid RecipientUserId,
    Guid FollowerUserId,
    string FollowerUserName);
