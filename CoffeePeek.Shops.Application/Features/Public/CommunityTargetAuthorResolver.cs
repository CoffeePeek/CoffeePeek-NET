using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;

namespace CoffeePeek.Shops.Application.Features.Public;

public static class CommunityTargetAuthorResolver
{
    public static async Task<Guid?> ResolveAuthorUserIdAsync(
        CommentTargetType targetType,
        Guid targetId,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        IQueryCommunityPostRepository postRepository,
        CancellationToken ct)
    {
        return targetType switch
        {
            CommentTargetType.Review => await ResolveReviewAuthorAsync(targetId, reviewRepository, ct),
            CommentTargetType.CheckIn => await ResolveCheckInAuthorAsync(targetId, checkInRepository, ct),
            CommentTargetType.Post => await ResolvePostAuthorAsync(targetId, postRepository, ct),
            _ => null
        };
    }

    public static async Task<Guid?> ResolveAuthorUserIdAsync(
        ReactionTargetType targetType,
        Guid targetId,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        IQueryCommunityPostRepository postRepository,
        CancellationToken ct) =>
        await ResolveAuthorUserIdAsync(
            (CommentTargetType)(int)targetType,
            targetId,
            reviewRepository,
            checkInRepository,
            postRepository,
            ct);

    private static async Task<Guid?> ResolveReviewAuthorAsync(
        Guid reviewId,
        IQueryReviewRepository reviewRepository,
        CancellationToken ct)
    {
        var review = await reviewRepository.GetById(reviewId, ct);
        return review is null || review.IsSoftDelete ? null : review.UserId;
    }

    private static async Task<Guid?> ResolveCheckInAuthorAsync(
        Guid checkInId,
        IQueryCheckInRepository checkInRepository,
        CancellationToken ct) =>
        await checkInRepository.GetUserIdByIdAsync(checkInId, ct);

    private static async Task<Guid?> ResolvePostAuthorAsync(
        Guid postId,
        IQueryCommunityPostRepository postRepository,
        CancellationToken ct)
    {
        var post = await postRepository.GetByIdAsync(postId, ct);
        return post is null || post.IsSoftDelete ? null : post.UserId;
    }
}
