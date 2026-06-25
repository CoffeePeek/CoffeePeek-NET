using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.Public;

public record CommunityNotificationDto
{
    public Guid Id { get; init; }
    public CommunityNotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Guid? RelatedUserId { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public Guid? CommentId { get; init; }
    public CommunityReactionType? ReactionType { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
