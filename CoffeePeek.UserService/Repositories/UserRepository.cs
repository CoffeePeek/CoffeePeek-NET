using CoffeePeek.UserService.Configuration;
using CoffeePeek.UserService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.Repositories;

public class UserRepository(UserDbContext context) : IUserRepository
{
    private readonly DbSet<User> _users = context.Users;
    private readonly UserDbContext _context = context;

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _users.Where(u => !u.IsSoftDelete).ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        await _users.AddAsync(user);
    }

    public Task UpdateAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _users.Update(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}