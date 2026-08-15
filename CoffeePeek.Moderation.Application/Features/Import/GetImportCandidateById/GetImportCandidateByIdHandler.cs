using CoffeePeek.Contract.Dtos.Import;
using CoffeePeek.Moderation.Domain.Aggregates.ShopImportCandidateAggregate;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Moderation.Application.Features.Import.GetImportCandidateById;

public record GetImportCandidateByIdQuery(Guid Id);

public static class GetImportCandidateByIdHandler
{
    public static async Task<Response<ShopImportCandidateDto>> Handle(
        GetImportCandidateByIdQuery query,
        IShopImportCandidateRepository repository,
        CancellationToken ct)
    {
        var candidate = await repository.GetByIdAsync(query.Id, ct);
        return candidate is null
            ? Response<ShopImportCandidateDto>.Error(System.Net.HttpStatusCode.NotFound, "Import candidate not found.")
            : Response<ShopImportCandidateDto>.Success(ShopImportCandidateMapper.ToDto(candidate));
    }
}
