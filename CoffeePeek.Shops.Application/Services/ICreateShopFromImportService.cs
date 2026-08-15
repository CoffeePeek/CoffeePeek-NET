using CoffeePeek.Contract.Events.Moderation;

namespace CoffeePeek.Shops.Application.Services;

public interface ICreateShopFromImportService
{
    Task<Guid> CreateShopFromImportAsync(ImportCandidatePublishedItem item, CancellationToken cancellationToken = default);
}
