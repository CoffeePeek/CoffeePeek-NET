using CoffeePeek.Contract.Enums;
using CoffeePeek.Shops.Domain.Aggregates.CommunityReactionAggregate;

namespace CoffeePeek.Shops.Application.Features.Public.Reactions;

public static class CommunityReactionTypeMapper
{
    public static CommunityReactionType ToDomain(Contract.Enums.CommunityReactionType reactionType) =>
        (CommunityReactionType)(int)reactionType;

    public static Contract.Enums.CommunityReactionType ToContract(CommunityReactionType reactionType) =>
        (Contract.Enums.CommunityReactionType)(int)reactionType;
}
