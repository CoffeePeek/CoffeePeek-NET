using CoffeePeek.UserService.Models;
using CoffeePeek.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.Repositories;

public class UserRepository(IGenericRepository<User> userRepository) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await userRepository.GetByIdAsync(userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await userRepository.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await userRepository.Query()
            .Where(u => !u.IsSoftDelete)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        await userRepository.AddAsync(user);
    }

    public Task UpdateAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        userRepository.Update(user);
        return Task.CompletedTask;
    }

}