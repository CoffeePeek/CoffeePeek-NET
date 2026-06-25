using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shops.Domain;

namespace CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;

public sealed partial class CommunityPost
{
    public static CommunityPost Create(
        Guid userId,
        string userName,
        CommunityPostType postType,
        string title,
        string body,
        Guid? linkedShopId,
        Guid moderationPostId)
    {
        if (userId == Guid.Empty)
            throw new DomainException($"{nameof(userId)} cannot be empty.");

        if (moderationPostId == Guid.Empty)
            throw new DomainException($"{nameof(moderationPostId)} cannot be empty.");

        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("UserName is required.");

        if (!Enum.IsDefined(postType))
            throw new DomainException("Invalid post type.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Post title is required.");

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length is < BusinessConstants.MinCommunityPostTitleLength
            or > BusinessConstants.MaxCommunityPostTitleLength)
        {
            throw new DomainException(
                $"Title must be between {BusinessConstants.MinCommunityPostTitleLength} and {BusinessConstants.MaxCommunityPostTitleLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Post body is required.");

        var trimmedBody = body.Trim();
        if (trimmedBody.Length is < BusinessConstants.MinCommunityPostBodyLength
            or > BusinessConstants.MaxCommunityPostBodyLength)
        {
            throw new DomainException(
                $"Body must be between {BusinessConstants.MinCommunityPostBodyLength} and {BusinessConstants.MaxCommunityPostBodyLength} characters.");
        }

        if (linkedShopId is { } shopId && shopId == Guid.Empty)
            throw new DomainException($"{nameof(linkedShopId)} cannot be empty.");

        return new CommunityPost(
            userId,
            userName.Trim(),
            postType,
            trimmedTitle,
            trimmedBody,
            linkedShopId,
            moderationPostId);
    }

    public void SoftDelete() => IsSoftDelete = true;
}
