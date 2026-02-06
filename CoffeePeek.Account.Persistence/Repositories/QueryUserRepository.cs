using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Infrastructure.Abstract;

namespace CoffeePeek.Account.Persistence.Repositories;

public class QueryUserRepository(IGenericRepository<User> userRepository) : IQueryUserRepository
{
    public Task<bool> UserExistsByEmail(string requestEmail, CancellationToken cancellationToken)
    {
        return userRepository.AnyAsync(c => c.Credentials.Email == requestEmail, cancellationToken);
    }
}