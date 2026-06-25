using CoffeePeek.Shared.Domain.Entities;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Account.Domain.Entities.CommunityNotificationAggregate;

public enum CommunityNotificationType
{
    NewComment = 1,
    NewReaction = 2,
    NewFollower = 3
}

public sealed class CommunityNotification : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public CommunityNotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public Guid? RelatedUserId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public Guid? CommentId { get; private set; }
    public int? ReactionType { get; private set; }
    public string? DedupKey { get; private set; }
    public bool IsRead { get; private set; }

    private CommunityNotification() { }

    private CommunityNotification(
        Guid userId,
        CommunityNotificationType type,
        string title,
        string message,
        Guid? relatedUserId,
        string? relatedEntityType,
        Guid? relatedEntityId,
        Guid? commentId,
        int? reactionType,
        string? dedupKey)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        RelatedUserId = relatedUserId;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        CommentId = commentId;
        ReactionType = reactionType;
        DedupKey = dedupKey;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static CommunityNotification Create(
        Guid userId,
        CommunityNotificationType type,
        string title,
        string message,
        Guid? relatedUserId = null,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        Guid? commentId = null,
        int? reactionType = null,
        string? dedupKey = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("Message is required.");

        return new CommunityNotification(
            userId,
            type,
            title.Trim(),
            message.Trim(),
            relatedUserId,
            relatedEntityType,
            relatedEntityId,
            commentId,
            reactionType,
            dedupKey);
    }

    public void MarkRead() => IsRead = true;
}
