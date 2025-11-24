using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Enums.Shop;

namespace CoffeePeek.BusinessLogic.Models;

public class ReviewShopModel(ModerationShop entity) : BaseModel<ModerationShop>(entity)
{
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