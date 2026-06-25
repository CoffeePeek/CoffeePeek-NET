using CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate;
using DomainPostType = CoffeePeek.Shops.Domain.Aggregates.CommunityPostAggregate.CommunityPostType;

namespace CoffeePeek.Shops.Application.Features.Public.Posts;

public static class CommunityPostTypeMapper
{
    public static DomainPostType ToDomain(Contract.Enums.CommunityPostType postType) =>
        (DomainPostType)(int)postType;

    public static Contract.Enums.CommunityPostType ToContract(DomainPostType postType) =>
        (Contract.Enums.CommunityPostType)(int)postType;
}
