using CoffeePeek.Contract.Enums;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.ModerationService.Models;
using CoffeePeek.ModerationService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ModerationService.Repositories;

public class ModerationShopRepository(IGenericRepository<ModerationShop> shopRepository) : IModerationShopRepository
{
    public async Task<ModerationShop?> GetByIdAsync(int id)
    {
        return await shopRepository.Query()
            .Include(s => s.ShopContacts)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .Include(s => s.ScheduleExceptions)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<ModerationShop>> GetByUserIdAsync(Guid userId)
    {
        return await shopRepository.Query()
            .Include(s => s.ShopContacts)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<ModerationShop>> GetAllAsync()
    {
        return await shopRepository.Query()
            .Include(s => s.ShopContacts)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .ToListAsync();
    }

    public async Task<List<ModerationShop>> GetByStatusAsync(ModerationStatus status)
    {
        return await shopRepository.Query()
            .Include(s => s.ShopContacts)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
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


