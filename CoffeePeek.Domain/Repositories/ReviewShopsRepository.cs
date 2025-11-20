using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Domain.Repositories;

public class ReviewShopsRepository(CoffeePeekDbContext context) : Repository<ModerationShop>(context)
{
    public async Task<bool> UpdatePhotos(int shopId, int userId, ICollection<string> urls)
    {
        var shop = await context.ReviewShops.FirstOrDefaultAsync(x => x.Id == shopId);
        
        if (shop == null)
        {
            return false;
        }

        var photos = urls.Select(x => new ShopPhoto
        {
            CreatedAt = DateTime.Now,
            Url = x,
            ShopId = shopId,
            UserId = userId,
        });

        foreach (var photo in photos)
        {
            shop.ShopPhotos.Add(photo);   
        }
        
        await context.SaveChangesAsync();
        
        return true;
    }
}