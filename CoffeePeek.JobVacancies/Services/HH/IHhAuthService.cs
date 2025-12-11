namespace CoffeePeek.JobVacancies.Services;

public interface IHhAuthService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}