using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Moderation.Domain.Aggregates.ModerationCommunityPostAggregate;
using CoffeePeek.Moderation.Domain.Common.Enums;
using DomainModerationStatus = CoffeePeek.Moderation.Domain.Common.Enums.ModerationStatus;
using CoffeePeek.Shared.Kernel.Response;
using MapsterMapper;

namespace CoffeePeek.Moderation.Application.Features.CommunityPost.GetAllModerationCommunityPosts;

public static class GetAllModerationCommunityPostsHandler
{
    public static async Task<Response<GetAllModerationCommunityPostsResponse>> Handle(
        GetAllModerationCommunityPostsQuery query,
        IQueryModerationCommunityPostRepository repository,
        IMapper mapper,
        CancellationToken ct)
    {
        DomainModerationStatus? domainStatus = query.Status.HasValue
            ? (DomainModerationStatus?)query.Status.Value
            : null;

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, totalCount) = await repository.GetPagedAsync(
            page, pageSize, domainStatus, query.Search, ct);

        var dtos = mapper.Map<ModerationCommunityPostDto[]>(items);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return Response<GetAllModerationCommunityPostsResponse>.Success(
            new GetAllModerationCommunityPostsResponse(dtos, totalCount, totalPages, page, pageSize));
    }
}
