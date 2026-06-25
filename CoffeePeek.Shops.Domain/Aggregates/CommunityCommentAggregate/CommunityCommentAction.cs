using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain;

namespace CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;

public sealed partial class CommunityComment
{
    public static CommunityComment Create(
        Guid userId,
        string userName,
        string body,
        CommentTargetType targetType,
        Guid targetId,
        Guid? parentCommentId = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException($"{nameof(userId)} cannot be empty.");

        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("UserName is required.");

        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Comment body is required.");

        var trimmedBody = body.Trim();
        if (trimmedBody.Length is < BusinessConstants.MinCommunityCommentBodyLength
            or > BusinessConstants.MaxCommunityCommentBodyLength)
        {
            throw new DomainException(
                $"Comment body must be between {BusinessConstants.MinCommunityCommentBodyLength} and {BusinessConstants.MaxCommunityCommentBodyLength} characters.");
        }

        if (targetId == Guid.Empty)
            throw new DomainException($"{nameof(targetId)} cannot be empty.");

        if (!Enum.IsDefined(targetType))
            throw new DomainException("Invalid comment target type.");

        return new CommunityComment(userId, userName.Trim(), trimmedBody, targetType, targetId, parentCommentId);
    }

    public void SoftDelete() => IsSoftDelete = true;
}
