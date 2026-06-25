using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;

public sealed partial class CommunityComment : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string UserName { get; private set; }
    public string Body { get; private set; }
    public CommentTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public Guid? ParentCommentId { get; private set; }
    public bool IsSoftDelete { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private CommunityComment()
    {
        UserName = string.Empty;
        Body = string.Empty;
    }

    private CommunityComment(
        Guid userId,
        string userName,
        string body,
        CommentTargetType targetType,
        Guid targetId,
        Guid? parentCommentId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        UserName = userName;
        Body = body;
        TargetType = targetType;
        TargetId = targetId;
        ParentCommentId = parentCommentId;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
