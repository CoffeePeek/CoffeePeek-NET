using CoffeePeek.Domain.Entities.Shop;
using CoffeePeek.Domain.UnitOfWork;

namespace CoffeePeek.Domain.Repositories.Interfaces;

public interface IModerationShopsRepository : IRepository<ModerationShop>
{
    Task<bool> UpdatePhotos(int shopId, int userId, ICollection<string> urls);
};