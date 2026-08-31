using System.Net;
using System.Text.Json.Serialization;
using CoffeePeek.Contract.Dtos.Shop;
using CoffeePeek.Shared.Domain.Interfaces.Infrastructure;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using MapsterMapper;

namespace CoffeePeek.Shops.Application.Features.Admin.Catalogs.Roasters;

public record CreateRoasterCommand(string Name);

public static class CreateRoasterHandler
{
    public static async Task<Response<RoasterDto>> Handle(
        CreateRoasterCommand command,
        IRoasterRepository repository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var existing = await repository.GetByNameAsync(command.Name, ct);
        if (existing is not null)
            return Response<RoasterDto>.Error(HttpStatusCode.Conflict, "A roaster with this name already exists.");

        var roaster = new Roaster(command.Name);
        repository.Add(roaster);
        await unitOfWork.SaveChangesAsync(ct);
        await InvalidateRoasterCachesAsync(cacheService, ct);

        return Response<RoasterDto>.Success(mapper.Map<RoasterDto>(roaster));
    }

    internal static async Task InvalidateRoasterCachesAsync(ICacheService cacheService, CancellationToken ct)
    {
        await cacheService.RemoveByPattern(CacheKey.Roaster.ListPattern(), ct);
        await cacheService.RemoveByPattern(CacheKey.Shop.SearchPattern(), ct);
    }
}

public record UpdateRoasterCommand([property: JsonIgnore] Guid Id, string Name);

public static class UpdateRoasterHandler
{
    public static async Task<Response<RoasterDto>> Handle(
        UpdateRoasterCommand command,
        IRoasterRepository repository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var roaster = await repository.GetByIdAsync(command.Id, ct);
        if (roaster is null)
            return Response<RoasterDto>.Error(HttpStatusCode.NotFound, "Roaster not found.");

        roaster.Update(command.Name);
        await unitOfWork.SaveChangesAsync(ct);
        await CreateRoasterHandler.InvalidateRoasterCachesAsync(cacheService, ct);

        return Response<RoasterDto>.Success(mapper.Map<RoasterDto>(roaster));
    }
}

public record DeleteRoasterCommand([property: JsonIgnore] Guid Id);

public static class DeleteRoasterHandler
{
    public static async Task<Response> Handle(
        DeleteRoasterCommand command,
        IRoasterRepository repository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        CancellationToken ct)
    {
        var roaster = await repository.GetByIdAsync(command.Id, ct);
        if (roaster is null)
            return Response.Error((int)HttpStatusCode.NotFound, "Roaster not found.");

        repository.Remove(roaster);
        await unitOfWork.SaveChangesAsync(ct);
        await CreateRoasterHandler.InvalidateRoasterCachesAsync(cacheService, ct);

        return Response.Success(message: "Roaster deleted.");
    }
}
