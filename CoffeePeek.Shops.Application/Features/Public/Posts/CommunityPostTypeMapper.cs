using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Posts;

public static class CommunityPostTypeMapper
{
    public static CommunityPostType ToDomain(Contract.Enums.CommunityPostType postType) =>
        (CommunityPostType)(int)postType;

    public static Contract.Enums.CommunityPostType ToContract(CommunityPostType postType) =>
        (Contract.Enums.CommunityPostType)(int)postType;
}
