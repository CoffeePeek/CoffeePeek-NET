using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Moderation.Domain.Specifications;

public class DuplicateShopSpecification : Specification<ModerationShop>
{
    public DuplicateShopSpecification(string name, string address)
    {
        Criteria = shop => 
#pragma warning disable CA1862
            shop.Name.ToLower() == name.ToLower() && 
            shop.NotValidatedAddress.ToLower() == address.ToLower();
#pragma warning restore CA1862
    }
}