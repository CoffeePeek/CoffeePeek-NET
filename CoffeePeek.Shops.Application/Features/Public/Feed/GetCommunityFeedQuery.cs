using CoffeePeek.Contract.Dtos.Public;
using CoffeePeek.Contract.Enums;

namespace CoffeePeek.Shops.Application.Features.Public.Feed;

public record GetCommunityFeedQuery(
    int Page = 1,
    int PageSize = 20,
    CommunityFeedFilter Filter = CommunityFeedFilter.All);

public record GetCommunityFeedResponse(
    IReadOnlyList<CommunityFeedItemDto> Items,
    int TotalItems,
    int TotalPages,
    int CurrentPage,
    int PageSize,
    CommunityFeedFilter Filter);
