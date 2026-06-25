using CoffeePeek.Moderation.Domain.Common.Enums;
using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;

public sealed partial class ModerationCommunityPost : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string UserName { get; private set; }
    public ModerationCommunityPostType PostType { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public Guid? LinkedShopId { get; private set; }

    public string? RejectedReason { get; private set; }
    public Guid? ModeratedBy { get; private set; }
    public DateTime ModeratedAt { get; private set; }
    public ModerationStatus ModerationStatus { get; private set; }
    public bool IsSoftDelete { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private ModerationCommunityPost()
    {
        UserName = string.Empty;
        Title = string.Empty;
        Body = string.Empty;
    }

    private ModerationCommunityPost(
        Guid userId,
        string userName,
        ModerationCommunityPostType postType,
        string title,
        string body,
        Guid? linkedShopId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        UserName = userName;
        PostType = postType;
        Title = title;
        Body = body;
        LinkedShopId = linkedShopId;
        ModerationStatus = ModerationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
