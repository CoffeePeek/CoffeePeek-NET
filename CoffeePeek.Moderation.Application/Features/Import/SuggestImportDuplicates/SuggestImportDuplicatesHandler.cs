using CoffeePeek.Contract.Dtos.Import;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Moderation.Domain.Import;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Import.SuggestImportDuplicates;

public record SuggestImportDuplicatesCommand;

public static class SuggestImportDuplicatesHandler
{
    public static async Task<Response<RefreshImportDuplicatesResultDto>> Handle(
        SuggestImportDuplicatesCommand command,
        IShopImportCandidateRepository candidates,
        IShopImportDuplicateSuggestionRepository suggestions,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var all = await candidates.ListForDuplicateScanAsync(ct);
        var suggested = await ImportDuplicateScan.AddNewAsync(all, suggestions, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var already = await suggestions.ListPairKeysAsync(ct);
        return Response<RefreshImportDuplicatesResultDto>.Success(
            new RefreshImportDuplicatesResultDto(all.Count, suggested, already.Count - suggested));
    }
}

public static class ImportDuplicateScan
{
    public static async Task<int> AddNewAsync(
        IReadOnlyList<ShopImportCandidate> candidates,
        IShopImportDuplicateSuggestionRepository suggestions,
        CancellationToken ct)
    {
        var existingPairs = await suggestions.ListPairKeysAsync(ct);
        var created = ImportDuplicateScanner.Scan(candidates, existingPairs);
        if (created.Count > 0)
            suggestions.AddRange(created);

        return created.Count;
    }
}
