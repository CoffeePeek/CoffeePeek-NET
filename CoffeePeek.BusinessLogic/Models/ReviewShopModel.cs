using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Enums.Shop;

namespace CoffeePeek.BusinessLogic.Models;

internal class ReviewShopModel : BaseModel<ModerationShop>
{
    public ReviewShopModel(ModerationShop entity) : base(entity)
    {
    }

    public void Update(ModerationShop entity)
    {
        
    }

    public void UpdateStatus(ModerationStatus requestModerationStatus)
    {
        if (Entity.ModerationStatus == requestModerationStatus)
        {
            return;
        }
        
        Entity.ModerationStatus = requestModerationStatus;
    }
}