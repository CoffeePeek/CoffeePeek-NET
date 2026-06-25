using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.Public;

public record ModerationCommunityPostDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public CommunityPostType PostType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public Guid? LinkedShopId { get; init; }
    public string? RejectedReason { get; init; }
    public Guid? ModeratedBy { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? ModeratedAt { get; init; }
    public ModerationStatus ModerationStatus { get; init; }
}
