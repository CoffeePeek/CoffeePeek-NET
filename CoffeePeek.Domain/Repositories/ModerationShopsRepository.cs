using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.Repositories.Interfaces;
using CoffeePeek.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Domain.Repositories;

public class ModerationShopsRepository(CoffeePeekDbContext context) : Repository<ModerationShop>(context), IModerationShopsRepository
{
    public async Task<bool> UpdatePhotos(int shopId, int userId, ICollection<string> urls)
    {
        var shop = await context.ModerationShops.FirstOrDefaultAsync(x => x.Id == shopId);
        
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