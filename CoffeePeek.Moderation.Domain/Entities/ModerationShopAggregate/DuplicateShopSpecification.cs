using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Entities;

public class DuplicateShopSpecification : Specification<ModerationShop>
{
    public DuplicateShopSpecification(string name, string address)
    {
        Criteria = shop =>
#pragma warning disable CA1862
            shop.Name.ToLower() == name.ToLower() && shop.Location!.Address.ToLower() == name.ToLower();
#pragma warning restore CA1862
    }
}