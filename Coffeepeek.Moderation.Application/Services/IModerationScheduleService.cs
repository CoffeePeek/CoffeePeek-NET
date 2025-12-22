using CoffeePeek.Contract.Dtos.Schedule;

namespace CoffeePeek.ModerationService.Services.Interfaces;

public interface IModerationScheduleService
{
    Task AddSchedulesAsync(
        Guid shopId,
        IReadOnlyCollection<ScheduleDto>? schedules,
        CancellationToken cancellationToken);
}