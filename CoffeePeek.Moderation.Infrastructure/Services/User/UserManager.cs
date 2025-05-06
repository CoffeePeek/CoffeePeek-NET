using CoffeePeek.Domain.UnitOfWork;
using CoffeePeek.Moderation.Infrastructure.Services.User.Interfaces;
using CoffeePeek.Shared.Extensions.Hashing;

namespace CoffeePeek.Moderation.Infrastructure.Services.User;

public class UserManager(IHashingService hashingService, IRepository<Domain.Entities.Users.User> userRepository) : IUserManager
{
    public bool CheckPasswordAsync(Domain.Entities.Users.User user, string requestPassword)
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
}