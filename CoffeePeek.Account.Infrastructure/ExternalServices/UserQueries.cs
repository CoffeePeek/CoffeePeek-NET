using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Auth.Infrastructure.Persistent;
using CoffeePeek.Contract.Dtos.User;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Auth.Infrastructure.ExternalServices;

public class UserQueries(AccountDbContext dbContext, IMapper mapper) : IUserQueries
{
    public async Task<UserDto?> GetProfileByIdAsync(Guid userId, CancellationToken ct)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(x => x.UserCredentialId == userId)
            .ProjectToType<UserDto>(mapper.Config)
            .FirstOrDefaultAsync(ct);
    }
}