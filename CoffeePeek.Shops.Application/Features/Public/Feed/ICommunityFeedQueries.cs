using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Shops.Application.Features.Public.Feed;

public interface ICommunityFeedQueries
{
    Task<(IReadOnlyList<CommunityFeedItemDto> Items, int TotalCount)> GetFeedAsync(
        int page,
        int pageSize,
        CommunityFeedFilter filter,
        CancellationToken cancellationToken = default);
}
