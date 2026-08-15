using CoffeePeek.Contract.Responses;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Shared.Kernel;

namespace CoffeePeek.Moderation.Infrastructure.Consumers;

public static class ImportCandidatePublishCompleteHandler
{
    public static async Task Handle(
        ImportCandidatePublishCompleteResponse message,
        IShopImportCandidateRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        foreach (var result in message.Results)
        {
            var candidate = await repository.GetByIdAsync(result.CandidateId, ct);
            if (candidate is null)
                continue;

            candidate.AttachPublishedShop(result.ShopId);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
