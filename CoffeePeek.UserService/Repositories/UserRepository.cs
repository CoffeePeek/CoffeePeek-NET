using CoffeePeek.UserService.Configuration;
using CoffeePeek.UserService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.Repositories;

public class UserRepository(UserDbContext context) : IUserRepository
{
    private readonly DbSet<User> _context = context.Users;

    public async Task<User?> GetByIdAsync(Guid eventUserId)
    {
        return await _context.FirstOrDefaultAsync(u => u.Id == eventUserId);
    }

    public async Task AddAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        await _context.AddAsync(user);
    }
}