using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;
using CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.Public.Comments;

public static class GetCommunityCommentsHandler
{
    public static async Task<Response<GetCommunityCommentsResponse>> Handle(
        GetCommunityCommentsQuery query,
        IQueryCommunityCommentRepository commentRepository,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        IQueryCommunityPostRepository postRepository,
        IMapper mapper,
        CancellationToken ct)
    {
        if (query.TargetId == Guid.Empty)
            throw new ValidationException("TargetId is required.");

        var domainTargetType = CommentTargetTypeMapper.ToDomain(query.TargetType);
        if (!await TargetExistsAsync(domainTargetType, query.TargetId, reviewRepository, checkInRepository, postRepository, ct))
            throw new NotFoundException("Feed item not found.");

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var (topLevelComments, totalCount) = await commentRepository.GetThreadPageAsync(
            domainTargetType, query.TargetId, page, pageSize, ct);

        var replies = await commentRepository.GetRepliesByParentIdsAsync(
            topLevelComments.Select(c => c.Id), ct);

        var repliesByParent = replies
            .GroupBy(r => r.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(c => mapper.Map<CommunityCommentDto>(c)).ToList());

        var items = topLevelComments
            .Select(comment =>
            {
                var dto = mapper.Map<CommunityCommentDto>(comment);
                var commentReplies = repliesByParent.GetValueOrDefault(comment.Id, []);
                return dto with { Replies = commentReplies };
            })
            .ToList();

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return Response<GetCommunityCommentsResponse>.Success(new GetCommunityCommentsResponse(
            items,
            totalCount,
            totalPages,
            page,
            pageSize));
    }

    private static async Task<bool> TargetExistsAsync(
        CommentTargetType targetType,
        Guid targetId,
        IQueryReviewRepository reviewRepository,
        IQueryCheckInRepository checkInRepository,
        IQueryCommunityPostRepository postRepository,
        CancellationToken ct) =>
        targetType switch
        {
            CommentTargetType.Review => await ReviewExistsAsync(targetId, reviewRepository, ct),
            CommentTargetType.CheckIn => await checkInRepository.ExistsByIdAsync(targetId, ct),
            CommentTargetType.Post => await postRepository.ExistsByIdAsync(targetId, ct),
            _ => false
        };

    private static async Task<bool> ReviewExistsAsync(
        Guid reviewId,
        IQueryReviewRepository reviewRepository,
        CancellationToken ct)
    {
        var review = await reviewRepository.GetById(reviewId, ct);
        return review is not null && !review.IsSoftDelete;
    }
}
