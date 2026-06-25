using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;
using DomainReactionType = CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate.CommunityReactionType;

namespace CoffeePeek.Shops.Application.Features.Public.Reactions;

public static class CommunityReactionTypeMapper
{
    public static DomainReactionType ToDomain(Contract.Enums.CommunityReactionType reactionType) =>
        (DomainReactionType)(int)reactionType;

    public static Contract.Enums.CommunityReactionType ToContract(DomainReactionType reactionType) =>
        (Contract.Enums.CommunityReactionType)(int)reactionType;
}
