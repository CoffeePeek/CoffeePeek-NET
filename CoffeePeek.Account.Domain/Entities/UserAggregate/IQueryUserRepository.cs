namespace CoffeePeek.Account.Domain.Entities.UserAggregate;

public interface IQueryUserRepository
{
    Task<bool> UserExistsByEmail(string requestEmail, CancellationToken cancellationToken);
}