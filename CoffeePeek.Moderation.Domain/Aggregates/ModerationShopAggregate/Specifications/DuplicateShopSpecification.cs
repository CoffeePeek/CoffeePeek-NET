using System.Linq.Expressions;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;

namespace CoffeePeek.Moderation.Domain.Aggregates;

public class DuplicateShopSpecification : ISpecification<ModerationShop>
{
    public DuplicateShopSpecification(string name, string address)
    {
        Criteria = shop =>
#pragma warning disable CA1862
            shop.Name.ToLower() == name.ToLower() && shop.Location!.Address.ToLower() == address.ToLower();
#pragma warning restore CA1862
    }

    public Expression<Func<ModerationShop, bool>> Criteria { get; }
}
