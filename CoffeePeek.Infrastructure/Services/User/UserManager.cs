using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Repositories;
using CoffeePeek.Infrastructure.Services.User.Interfaces;

namespace CoffeePeek.Infrastructure.Services.User;

public class UserManager(UserRepository userRepository, 
    IHashingService hashingService) : IUserManager
{
    public bool CheckPassword(Domain.Entities.Users.User user, string requestPassword)
    {
        return hashingService.VerifyHashedStrings(requestPassword, user.PasswordHash);
    }

    public Task<Domain.Entities.Users.User?> FindByEmailAsync(string requestEmail)
    {
        return userRepository.FirstOrDefaultAsync(x => x.Email == requestEmail);
    }

    public async Task CreateAsync(Domain.Entities.Users.User user, string requestPassword)
    {
        user.PasswordHash = hashingService.HashString(requestPassword);
        userRepository.Add(user);

        await userRepository.SaveChangesAsync();
    }

    public async Task<ICollection<Role>> GetRolesAsync(Domain.Entities.Users.User user)
    {
        return await userRepository.GetUserRoles(user);
    }
}