namespace CoffeePeek.JobVacancies.Application.Services;

public interface IHhAuthService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}