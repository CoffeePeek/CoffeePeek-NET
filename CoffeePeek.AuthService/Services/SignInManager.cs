using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;

namespace CoffeePeek.AuthService.Services;

public class SignInManager(IUserManager userManager, ISessionManager sessionManager, ILogger<SignInManager> logger) : ISignInManager
{
    public async Task<SignInResultWrapper> CheckPasswordSignInAsync(UserCredentials userCredentials,
        string requestPassword)
    {
        var user = await userManager.FindByEmailAsync(userCredentials.Email);

        if (user == null)
        {
            logger.LogWarning("Authentication failed: User with email {Email} not found.", userCredentials.Email);
            return SignInResultWrapper.Failed;
        }
        
        var passwordIsValid = await userManager.CheckPasswordAsync(user, requestPassword);

        if (!passwordIsValid)
        {
            logger.LogWarning("Authentication failed: Invalid password for user {Email}.", userCredentials.Email);
            return SignInResultWrapper.Failed;
        }

        await sessionManager.SignInAsync(user);
        logger.LogInformation("User {Email} signed in successfully.", userCredentials.Email);

        return SignInResultWrapper.Success;
    }
}