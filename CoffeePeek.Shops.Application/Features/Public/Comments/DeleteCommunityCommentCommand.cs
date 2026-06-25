using System.Text.Json.Serialization;

namespace CoffeePeek.Shops.Application.Features.Public.Comments;

public record DeleteCommunityCommentCommand(
    Guid CommentId,
    [property: JsonIgnore] Guid RequestingUserId,
    [property: JsonIgnore] bool CanModerate);
