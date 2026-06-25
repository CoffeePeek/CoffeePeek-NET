using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Shops.Application.Features.Public.Feed;

public record CommunityFeedQueryContext(
    Guid? CityId = null,
    Guid? ViewerUserId = null,
    IReadOnlyList<Guid>? FollowingUserIds = null,
    HashSet<Guid>? CityShopIds = null);

public interface ICommunityFeedQueries
{
    Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetFeedAsync(
        int page,
        int pageSize,
        CommunityFeedFilter filter,
        CommunityFeedQueryContext context,
        CancellationToken cancellationToken = default);
}
