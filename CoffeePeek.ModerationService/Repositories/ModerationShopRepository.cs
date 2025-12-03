using CoffeePeek.Domain.Enums.Shop;
using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ModerationService.Repositories;

public class ModerationShopRepository(ModerationDbContext context) : IModerationShopRepository
{
    private readonly DbSet<ModerationShop> _shops = context.ModerationShops;
    private readonly ModerationDbContext _context = context;

    public async Task<ModerationShop?> GetByIdAsync(int id)
    {
        return await _shops
            .Include(s => s.Address)
            .Include(s => s.ShopContacts)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .Include(s => s.ScheduleExceptions)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<ModerationShop>> GetByUserIdAsync(Guid userId)
    {
        return await _shops
            .Include(s => s.Address)
            .Include(s => s.ShopContacts)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<ModerationShop>> GetAllAsync()
    {
        return await _shops
            .Include(s => s.Address)
            .Include(s => s.ShopContacts)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .ToListAsync();
    }

    public async Task<List<ModerationShop>> GetByStatusAsync(ModerationStatus status)
    {
        return await _shops
            .Include(s => s.Address)
            .Include(s => s.ShopContacts)
            .Include(s => s.ShopPhotos)
            .Include(s => s.Schedules)
            .Where(s => s.ModerationStatus == status)
            .ToListAsync();
    }

    public async Task<ModerationShop?> GetByNameAndAddressAsync(string name, string address, Guid userId)
    {
        return await _shops
            .FirstOrDefaultAsync(s => s.Name == name 
                && s.NotValidatedAddress == address 
                && s.UserId == userId 
                && s.ModerationStatus == ModerationStatus.Pending);
    }

    public async Task AddAsync(ModerationShop shop)
    {
        ArgumentNullException.ThrowIfNull(shop);
        await _shops.AddAsync(shop);
    }

    public Task UpdateAsync(ModerationShop shop)
    {
        ArgumentNullException.ThrowIfNull(shop);
        _shops.Update(shop);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

