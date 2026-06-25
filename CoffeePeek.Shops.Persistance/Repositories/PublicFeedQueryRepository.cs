using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Application.Features.Public.Feed;
using CoffeePeek.Shops.Application.Features.Public.Posts;
using CoffeePeek.Shops.Application.Features.Public.Reactions;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityCommentAggregate;
using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;
using CoffeePeek.Shops.Persistance.Configuration;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using CheckIn = CoffeePeek.Shops.Domain.Aggregates.CheckInAggregate.CheckIn;
using CommunityPost = CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate.CommunityPost;
using Review = CoffeePeek.Shops.Domain.Aggregates.ReviewAggregate.Review;

namespace CoffeePeek.Shops.Persistance.Repositories;

public class PublicFeedQueryRepository(
    ShopsDbContext dbContext,
    IMapper mapper,
    IQueryCoffeeShopRepository coffeeShopRepository,
    IQueryCommunityCommentRepository commentRepository,
    IQueryCommunityReactionRepository reactionRepository) : ICommunityFeedQueries
{
    public async Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetFeedAsync(
        int page,
        int pageSize,
        CommunityFeedFilter filter,
        CommunityFeedQueryContext context,
        CancellationToken cancellationToken = default)
    {
        HashSet<Guid>? cityShopIds = null;
        if (context.CityId is { } cityId)
            cityShopIds = await coffeeShopRepository.GetShopIdsByCityIdAsync(cityId, cancellationToken);

        var viewerUserId = context.ViewerUserId;

        return filter switch
        {
            CommunityFeedFilter.Reviews => await GetReviewsFeedAsync(page, pageSize, cityShopIds, context.FollowingUserIds, viewerUserId, cancellationToken),
            CommunityFeedFilter.CheckIns => await GetCheckInsFeedAsync(page, pageSize, cityShopIds, context.FollowingUserIds, viewerUserId, cancellationToken),
            CommunityFeedFilter.Posts => await GetPostsFeedAsync(page, pageSize, cityShopIds, context.FollowingUserIds, viewerUserId, cancellationToken),
            CommunityFeedFilter.Following => await GetMergedFeedAsync(page, pageSize, cityShopIds, context.FollowingUserIds, viewerUserId, cancellationToken),
            _ => await GetMergedFeedAsync(page, pageSize, cityShopIds, context.FollowingUserIds, viewerUserId, cancellationToken)
        };
    }

    private async Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetReviewsFeedAsync(
        int page, int pageSize, HashSet<Guid>? cityShopIds, IReadOnlyList<Guid>? followingUserIds, Guid? viewerUserId, CancellationToken ct)
    {
        var query = dbContext.Reviews.AsNoTracking().Where(r => !r.IsSoftDelete);
        query = ApplyCityFilter(query, r => r.CoffeeShopId, cityShopIds);
        query = ApplyFollowingFilter(query, r => r.UserId, followingUserIds);

        var totalCount = await query.CountAsync(ct);
        var reviews = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.Photos)
            .ToListAsync(ct);

        var items = await EnrichFeedItemsAsync(
            reviews.Select(r => mapper.Map<CommunityFeedItemDto>(r)).ToList(), ct, viewerUserId);
        return (items, totalCount);
    }

    private async Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetCheckInsFeedAsync(
        int page, int pageSize, HashSet<Guid>? cityShopIds, IReadOnlyList<Guid>? followingUserIds, Guid? viewerUserId, CancellationToken ct)
    {
        var query = dbContext.CheckIns.AsNoTracking();
        query = ApplyCityFilter(query, c => c.ShopId, cityShopIds);
        query = ApplyFollowingFilter(query, c => c.UserId, followingUserIds);

        var totalCount = await query.CountAsync(ct);
        var checkIns = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.ShopPhotos)
            .ToListAsync(ct);

        var items = await EnrichFeedItemsAsync(
            checkIns.Select(c => mapper.Map<CommunityFeedItemDto>(c)).ToList(), ct, viewerUserId);
        return (items, totalCount);
    }

    private async Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetPostsFeedAsync(
        int page, int pageSize, HashSet<Guid>? cityShopIds, IReadOnlyList<Guid>? followingUserIds, Guid? viewerUserId, CancellationToken ct)
    {
        var query = dbContext.CommunityPosts.AsNoTracking().Where(p => !p.IsSoftDelete);
        if (cityShopIds is not null)
            query = query.Where(p => p.LinkedShopId != null && cityShopIds.Contains(p.LinkedShopId.Value));
        query = ApplyFollowingFilter(query, p => p.UserId, followingUserIds);

        var totalCount = await query.CountAsync(ct);
        var posts = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = await EnrichFeedItemsAsync(
            posts.Select(p => mapper.Map<CommunityFeedItemDto>(p)).ToList(), ct, viewerUserId);
        return (items, totalCount);
    }

    private async Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetMergedFeedAsync(
        int page, int pageSize, HashSet<Guid>? cityShopIds, IReadOnlyList<Guid>? followingUserIds, Guid? viewerUserId, CancellationToken ct)
    {
        var reviewQuery = dbContext.Reviews.AsNoTracking().Where(r => !r.IsSoftDelete);
        reviewQuery = ApplyCityFilter(reviewQuery, r => r.CoffeeShopId, cityShopIds);
        reviewQuery = ApplyFollowingFilter(reviewQuery, r => r.UserId, followingUserIds);

        var checkInQuery = dbContext.CheckIns.AsNoTracking();
        checkInQuery = ApplyCityFilter(checkInQuery, c => c.ShopId, cityShopIds);
        checkInQuery = ApplyFollowingFilter(checkInQuery, c => c.UserId, followingUserIds);

        var postQuery = dbContext.CommunityPosts.AsNoTracking().Where(p => !p.IsSoftDelete);
        if (cityShopIds is not null)
            postQuery = postQuery.Where(p => p.LinkedShopId != null && cityShopIds.Contains(p.LinkedShopId.Value));
        postQuery = ApplyFollowingFilter(postQuery, p => p.UserId, followingUserIds);

        var reviewCount = await reviewQuery.CountAsync(ct);
        var checkInCount = await checkInQuery.CountAsync(ct);
        var postCount = await postQuery.CountAsync(ct);
        var totalCount = reviewCount + checkInCount + postCount;

        if (totalCount == 0)
            return ([], 0);

        var window = page * pageSize;
        var reviews = await reviewQuery.OrderByDescending(r => r.CreatedAtUtc).Take(window).Include(r => r.Photos).ToListAsync(ct);
        var checkIns = await checkInQuery.OrderByDescending(c => c.CreatedAtUtc).Take(window).Include(c => c.ShopPhotos).ToListAsync(ct);
        var posts = await postQuery.OrderByDescending(p => p.CreatedAtUtc).Take(window).ToListAsync(ct);

        var merged = reviews.Select(r => mapper.Map<CommunityFeedItemDto>(r))
            .Concat(checkIns.Select(c => mapper.Map<CommunityFeedItemDto>(c)))
            .Concat(posts.Select(p => mapper.Map<CommunityFeedItemDto>(p)))
            .OrderByDescending(item => item.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = await EnrichFeedItemsAsync(merged, ct, context.ViewerUserId);
        return (items, totalCount);
    }

    private static IQueryable<T> ApplyCityFilter<T>(
        IQueryable<T> query,
        System.Linq.Expressions.Expression<Func<T, Guid>> shopIdSelector,
        HashSet<Guid>? cityShopIds)
    {
        if (cityShopIds is null)
            return query;

        if (cityShopIds.Count == 0)
            return query.Where(_ => false);

        return query.Where(BuildShopIdContainsExpression(shopIdSelector, cityShopIds));
    }

    private static IQueryable<T> ApplyFollowingFilter<T>(
        IQueryable<T> query,
        System.Linq.Expressions.Expression<Func<T, Guid>> userIdSelector,
        IReadOnlyList<Guid>? followingUserIds)
    {
        if (followingUserIds is null || followingUserIds.Count == 0)
            return query;

        return query.Where(BuildUserIdContainsExpression(userIdSelector, followingUserIds));
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> BuildShopIdContainsExpression<T>(
        System.Linq.Expressions.Expression<Func<T, Guid>> shopIdSelector,
        HashSet<Guid> cityShopIds)
    {
        var parameter = shopIdSelector.Parameters[0];
        var containsMethod = typeof(HashSet<Guid>).GetMethod(nameof(HashSet<Guid>.Contains), [typeof(Guid)])!;
        var call = System.Linq.Expressions.Expression.Call(
            System.Linq.Expressions.Expression.Constant(cityShopIds),
            containsMethod,
            shopIdSelector.Body);
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(call, parameter);
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> BuildUserIdContainsExpression<T>(
        System.Linq.Expressions.Expression<Func<T, Guid>> userIdSelector,
        IReadOnlyList<Guid> followingUserIds)
    {
        var set = followingUserIds.ToHashSet();
        var parameter = userIdSelector.Parameters[0];
        var containsMethod = typeof(HashSet<Guid>).GetMethod(nameof(HashSet<Guid>.Contains), [typeof(Guid)])!;
        var call = System.Linq.Expressions.Expression.Call(
            System.Linq.Expressions.Expression.Constant(set),
            containsMethod,
            userIdSelector.Body);
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(call, parameter);
    }

    private async Task<IReadOnlyList<CommunityFeedItemDto>> EnrichFeedItemsAsync(
        IReadOnlyList<CommunityFeedItemDto> items,
        CancellationToken ct,
        Guid? viewerUserId = null)
    {
        if (items.Count == 0)
            return items;

        var shopIds = items.Where(i => i.ShopId != Guid.Empty).Select(i => i.ShopId).Distinct();
        var shopNames = await coffeeShopRepository.GetShopNamesByIdsAsync(shopIds, ct);

        var reviewIds = items.Where(i => i.Type == CommunityFeedItemType.Review).Select(i => i.Id).ToArray();
        var checkInIds = items.Where(i => i.Type == CommunityFeedItemType.CheckIn).Select(i => i.Id).ToArray();
        var postIds = items.Where(i => i.Type == CommunityFeedItemType.Post).Select(i => i.Id).ToArray();

        var reviewCommentCounts = await commentRepository.GetCommentCountsByTargetsAsync(
            CommentTargetType.Review, reviewIds, ct);
        var checkInCommentCounts = await commentRepository.GetCommentCountsByTargetsAsync(
            CommentTargetType.CheckIn, checkInIds, ct);
        var postCommentCounts = await commentRepository.GetCommentCountsByTargetsAsync(
            CommentTargetType.Post, postIds, ct);

        var reviewReactions = await reactionRepository.GetCountsByTargetsAsync(ReactionTargetType.Review, reviewIds, ct);
        var checkInReactions = await reactionRepository.GetCountsByTargetsAsync(ReactionTargetType.CheckIn, checkInIds, ct);
        var postReactions = await reactionRepository.GetCountsByTargetsAsync(ReactionTargetType.Post, postIds, ct);

        Dictionary<Guid, CommunityReactionType>? viewerReviewReactions = null;
        Dictionary<Guid, CommunityReactionType>? viewerCheckInReactions = null;
        Dictionary<Guid, CommunityReactionType>? viewerPostReactions = null;
        if (viewerUserId is { } viewerId)
        {
            viewerReviewReactions = await reactionRepository.GetViewerReactionsByTargetsAsync(viewerId, ReactionTargetType.Review, reviewIds, ct);
            viewerCheckInReactions = await reactionRepository.GetViewerReactionsByTargetsAsync(viewerId, ReactionTargetType.CheckIn, checkInIds, ct);
            viewerPostReactions = await reactionRepository.GetViewerReactionsByTargetsAsync(viewerId, ReactionTargetType.Post, postIds, ct);
        }

        return items
            .Select(item =>
            {
                var reactionCounts = item.Type switch
                {
                    CommunityFeedItemType.Review => reviewReactions.GetValueOrDefault(item.Id),
                    CommunityFeedItemType.CheckIn => checkInReactions.GetValueOrDefault(item.Id),
                    CommunityFeedItemType.Post => postReactions.GetValueOrDefault(item.Id),
                    _ => null
                };

                CommunityReactionType? viewerReaction = item.Type switch
                {
                    CommunityFeedItemType.Review => viewerReviewReactions?.GetValueOrDefault(item.Id),
                    CommunityFeedItemType.CheckIn => viewerCheckInReactions?.GetValueOrDefault(item.Id),
                    CommunityFeedItemType.Post => viewerPostReactions?.GetValueOrDefault(item.Id),
                    _ => null
                };

                return item with
                {
                    ShopName = item.ShopId == Guid.Empty
                        ? string.Empty
                        : shopNames.GetValueOrDefault(item.ShopId, string.Empty),
                    CommentCount = item.Type switch
                    {
                        CommunityFeedItemType.Review => reviewCommentCounts.GetValueOrDefault(item.Id, 0),
                        CommunityFeedItemType.CheckIn => checkInCommentCounts.GetValueOrDefault(item.Id, 0),
                        CommunityFeedItemType.Post => postCommentCounts.GetValueOrDefault(item.Id, 0),
                        _ => 0
                    },
                    Reactions = reactionCounts is null
                        ? new CommunityReactionCountsDto()
                        : new CommunityReactionCountsDto
                        {
                            WantToTry = reactionCounts.WantToTry,
                            GreatFind = reactionCounts.GreatFind,
                            Helpful = reactionCounts.Helpful
                        },
                    ViewerReaction = viewerReaction is null
                        ? null
                        : CommunityReactionTypeMapper.ToContract(viewerReaction.Value)
                };
            })
            .ToList();
    }
}
