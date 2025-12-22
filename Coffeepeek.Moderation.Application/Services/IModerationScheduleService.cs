using CoffeePeek.Contract.Dtos.Schedule;

namespace Coffeepeek.Moderation.Application.Services;

public interface IModerationScheduleService
{
    Task AddSchedulesAsync(
        Guid shopId,
        IReadOnlyCollection<ScheduleDto>? schedules,
        CancellationToken cancellationToken);
}