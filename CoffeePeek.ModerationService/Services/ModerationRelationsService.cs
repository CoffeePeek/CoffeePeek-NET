using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Entities;
using CoffeePeek.ModerationService.Services.Interfaces;

namespace CoffeePeek.ModerationService.Services;

public class ModerationRelationsService(ModerationDbContext dbContext)
    : IModerationRelationsService
{
    public async Task AddEquipmentsAsync(Guid shopId, IReadOnlyCollection<Guid>? equipmentIds, CancellationToken cancellationToken)
    {
        if (equipmentIds is null || equipmentIds.Count == 0)
            return;

        foreach (var equipmentId in equipmentIds)
        {
            await dbContext.ModerationShopEquipments.AddAsync(new ModerationShopEquipment
            {
                Id = Guid.NewGuid(),
                ShopId = shopId,
                EquipmentId = equipmentId
            }, cancellationToken);
        }
    }

    public async Task AddCoffeeBeansAsync(Guid shopId, IReadOnlyCollection<Guid>? coffeeBeanIds, CancellationToken cancellationToken)
    {
        if (coffeeBeanIds is null || coffeeBeanIds.Count == 0)
            return;

        foreach (var beanId in coffeeBeanIds)
        {
            await dbContext.ModerationCoffeeBeanShops.AddAsync(new ModerationCoffeeBeanShop
            {
                Id = Guid.NewGuid(),
                ShopId = shopId,
                CoffeeBeanId = beanId
            }, cancellationToken);
        }
    }

    public async Task AddRoastersAsync(Guid shopId, IReadOnlyCollection<Guid>? roasterIds, CancellationToken cancellationToken)
    {
        if (roasterIds is null || roasterIds.Count == 0)
            return;

        foreach (var roasterId in roasterIds)
        {
            await dbContext.ModerationRoasterShops.AddAsync(new ModerationRoasterShop
            {
                Id = Guid.NewGuid(),
                ShopId = shopId,
                RoasterId = roasterId
            }, cancellationToken);
        }
    }

    public async Task AddBrewMethodsAsync(Guid shopId, IReadOnlyCollection<Guid>? brewMethodIds, CancellationToken cancellationToken)
    {
        if (brewMethodIds is null || brewMethodIds.Count == 0)
            return;

        foreach (var brewMethodId in brewMethodIds)
        {
            await dbContext.ModerationShopBrewMethods.AddAsync(new ModerationShopBrewMethod
            {
                Id = Guid.NewGuid(),
                ShopId = shopId,
                BrewMethodId = brewMethodId
            }, cancellationToken);
        }
    }
}