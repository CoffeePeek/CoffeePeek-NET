using CoffeePeek.Shared.Domain.Entities;

namespace CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;

public sealed partial class CommunityPost : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string UserName { get; private set; }
    public CommunityPostType PostType { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public Guid? LinkedShopId { get; private set; }
    public Guid ModerationPostId { get; private set; }
    public bool IsSoftDelete { get; private set; }

    // ReSharper disable once UnusedMember.Local
    private CommunityPost()
    {
        UserName = string.Empty;
        Title = string.Empty;
        Body = string.Empty;
    }

    private CommunityPost(
        Guid userId,
        string userName,
        CommunityPostType postType,
        string title,
        string body,
        Guid? linkedShopId,
        Guid moderationPostId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        UserName = userName;
        PostType = postType;
        Title = title;
        Body = body;
        LinkedShopId = linkedShopId;
        ModerationPostId = moderationPostId;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
