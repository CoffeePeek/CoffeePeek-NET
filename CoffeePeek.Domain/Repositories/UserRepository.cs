using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.Repositories.Interfaces;
using CoffeePeek.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Domain.Repositories;

public class UserRepository(CoffeePeekDbContext context) : Repository<User>(context), IUserRepository
{
    public virtual async Task<User?> GetUserByRefreshToken(string refreshToken)
    {
        var refreshTokenEntity = await context.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Token == refreshToken);

        return refreshTokenEntity?.User;
    }

    public async Task<ICollection<Role>> GetUserRoles(User user)
    {
        return await context.Roles
            .Where(x => x.UserRoles.Any(y => y.UserId == user.Id))
            .ToListAsync();
    }
}