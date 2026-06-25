namespace CoffeePeek.Contract.Dtos.Public;

public record CommunityCommentDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public Guid? ParentCommentId { get; init; }
    public IReadOnlyList<CommunityCommentDto> Replies { get; init; } = [];
}
