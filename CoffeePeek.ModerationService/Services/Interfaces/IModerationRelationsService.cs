namespace CoffeePeek.ModerationService.Services.Interfaces;

public interface IModerationRelationsService
{
    Task AddEquipmentsAsync(Guid shopId, IReadOnlyCollection<Guid>? equipmentIds, CancellationToken cancellationToken);
    Task AddCoffeeBeansAsync(Guid shopId, IReadOnlyCollection<Guid>? coffeeBeanIds, CancellationToken cancellationToken);
    Task AddRoastersAsync(Guid shopId, IReadOnlyCollection<Guid>? roasterIds, CancellationToken cancellationToken);
    Task AddBrewMethodsAsync(Guid shopId, IReadOnlyCollection<Guid>? brewMethodIds, CancellationToken cancellationToken);
}