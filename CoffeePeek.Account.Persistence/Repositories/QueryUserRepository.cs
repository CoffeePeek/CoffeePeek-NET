using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Account.Persistence.Repositories;

public class QueryUserRepository(AccountDbContext dbContext) : IQueryUserRepository
{
    private readonly DbSet<User> _repository = dbContext.Users;
    
    public void Add(User newUser, CancellationToken ct)
    {
        _repository.Add(newUser);
    }
    
    public Task<bool> UserExistsByEmail(string requestEmail, CancellationToken cancellationToken)
    {
        return _repository.AnyAsync(c => c.Credentials.Email == requestEmail, cancellationToken);
    }
    
    public Task<User?> GetByProvider(string provider, string providerId, CancellationToken ct)
    {
        return _repository
            .AsNoTracking()
            .Include(x => x.Credentials)
            .FirstOrDefaultAsync(x => x.Credentials.OAuthProvider == provider, cancellationToken: ct);
    }

    public Task<User?> GetByEmail(string email, CancellationToken ct)
    {
        return _repository
            .AsNoTracking()
            .Include(x => x.Credentials)
            .FirstOrDefaultAsync(x => x.Credentials.Email.Value == email, cancellationToken: ct);
    }

    public async Task<bool> IsEmailUnique(string email, CancellationToken ct)
    {
        return !await _repository.AnyAsync(c => c.Credentials.Email == email, ct);
    }
}