using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Enums.Shop;

namespace CoffeePeek.BusinessLogic.Models;

internal class ReviewShopModel : BaseModel<ReviewShop>
{
    public ReviewShopModel(ReviewShop entity) : base(entity)
    {
    }

    public void Update(ReviewShop entity)
    {
        
    }

    public void UpdateStatus(ReviewStatus requestReviewStatus)
    {
        if (Entity.ReviewStatus == requestReviewStatus)
        {
            return;
        }
        
        Entity.ReviewStatus = requestReviewStatus;
    }
}