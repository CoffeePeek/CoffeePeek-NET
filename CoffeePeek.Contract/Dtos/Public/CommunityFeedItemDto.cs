using CoffeePeek.Contract.Dtos;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Contract.Dtos.Public;

public record CommunityFeedItemDto
{
    public CommunityFeedItemType Type { get; init; }
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public Guid ShopId { get; init; }
    public string ShopName { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }

    public string? Header { get; init; }
    public string? Comment { get; init; }
    public string? Note { get; init; }
    public RatingDto? Rating { get; init; }
    public Guid? LinkedReviewId { get; init; }
    public int CommentCount { get; init; }

    public ICollection<ShortPhotoMetadataDto> Photos { get; init; } = [];
}
