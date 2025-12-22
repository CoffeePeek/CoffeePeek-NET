using CoffeePeek.Contract.Enums;
using CoffeePeek.Moderation.Domain.Entities;
using CoffeePeek.Moderation.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Moderation.Application.Repositories;

public class ModerationShopRepository(IGenericRepository<ModerationShop> shopRepository) : IModerationShopRepository
{
    public async Task<ModerationShop?> GetByIdAsync(Guid id)
    {
        return await shopRepository.Query()
            .Include(s => s.ShopContacts)
            .Include(s => s.Location)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
                .ThenInclude(sch => sch.Intervals)
            .Include(s => s.ModerationShopEquipments)
            .Include(s => s.ModerationCoffeeBeanShops)
            .Include(s => s.ModerationRoasterShops)
            .Include(s => s.ModerationShopBrewMethods)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<ModerationShop>> GetByUserIdAsync(Guid userId)
    {
        return await shopRepository.Query()
            .Include(s => s.ShopContacts)
            .Include(s => s.Location)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
                .ThenInclude(sch => sch.Intervals)
            .Include(s => s.ModerationShopEquipments)
            .Include(s => s.ModerationCoffeeBeanShops)
            .Include(s => s.ModerationRoasterShops)
            .Include(s => s.ModerationShopBrewMethods)
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<ModerationShop>> GetAllAsync()
    {
        return await shopRepository.Query()
            .Include(s => s.ShopContacts)
            .Include(s => s.Location)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
                .ThenInclude(sch => sch.Intervals)
            .Include(s => s.ModerationShopEquipments)
            .Include(s => s.ModerationCoffeeBeanShops)
            .Include(s => s.ModerationRoasterShops)
            .Include(s => s.ModerationShopBrewMethods)
            .ToListAsync();
    }

    public async Task<List<ModerationShop>> GetByStatusAsync(ModerationStatus status)
    {
        return await shopRepository.Query()
            .Include(s => s.ShopContacts)
            .Include(s => s.Location)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
                .ThenInclude(sch => sch.Intervals)
            .Include(s => s.ModerationShopEquipments)
            .Include(s => s.ModerationCoffeeBeanShops)
            .Include(s => s.ModerationRoasterShops)
            .Include(s => s.ModerationShopBrewMethods)
            .Where(s => s.ModerationStatus == status)
            .ToListAsync();
    }

    public async Task<ModerationShop?> GetByNameAndAddressAsync(string name, string address, Guid userId)
    {
        return await shopRepository.FirstOrDefaultAsync(s => s.Name == name 
            && s.NotValidatedAddress == address 
            && s.UserId == userId 
            && s.ModerationStatus == ModerationStatus.Pending);
    }

    public async Task<int> GetApprovedShopsCountByUserIdAsync(Guid userId)
    {
        return await shopRepository.Query()
            .CountAsync(s => s.UserId == userId 
                && s.ModerationStatus == ModerationStatus.Approved 
                && s.ShopId != null);
    }

    public async Task AddAsync(ModerationShop shop)
    {
        ArgumentNullException.ThrowIfNull(shop);
        await shopRepository.AddAsync(shop);
    }

    public Task UpdateAsync(ModerationShop shop)
    {
        ArgumentNullException.ThrowIfNull(shop);
        shopRepository.Update(shop);
        return Task.CompletedTask;
    }

}


