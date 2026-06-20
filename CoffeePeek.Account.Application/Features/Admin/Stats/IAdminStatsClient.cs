using CoffeePeek.Contract.Dtos.Admin;

namespace CoffeePeek.Account.Application.Features.Admin.Stats;

public interface IAdminStatsClient
{
    Task<AdminServiceStatsDto> GetPlatformStatsAsync(CancellationToken cancellationToken = default);
}
