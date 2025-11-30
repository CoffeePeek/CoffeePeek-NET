using CoffeePeek.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.Infrastructure.Services;

public class UserService(UserManager<User> userManager) : IUserService
{
    public async Task<User> GetOrCreateGoogleUserAsync(string googleId, string email, string name, string avatarUrl)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new User
            {
                Email = email,
                UserName = name,
                GoogleId = googleId,
                AvatarUrl = avatarUrl
            };

            await userManager.CreateAsync(user);
        }

        return user;
    }
}