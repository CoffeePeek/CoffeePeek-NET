using CoffeePeek.Contract.Dtos.Schedule;
using CoffeePeek.ModerationService.Configuration;
using CoffeePeek.ModerationService.Entities;
using CoffeePeek.ModerationService.Services.Interfaces;

namespace CoffeePeek.ModerationService.Services;

public class ModerationScheduleService(ModerationDbContext dbContext)
    : IModerationScheduleService
{
    public async Task AddSchedulesAsync(
        Guid shopId,
        IReadOnlyCollection<ScheduleDto>? schedules,
        CancellationToken cancellationToken)
    {
        if (schedules is null || schedules.Count == 0)
            return;

        foreach (var scheduleDto in schedules)
        {
            if (scheduleDto.DayOfWeek == null)
                continue;

            var schedule = new ModerationShopSchedule
            {
                Id = Guid.NewGuid(),
                ShopId = shopId,
                DayOfWeek = scheduleDto.DayOfWeek.Value,
                IsClosed = scheduleDto.Intervals == null || !scheduleDto.Intervals.Any()
            };

            if (!schedule.IsClosed && scheduleDto.Intervals != null)
            {
                schedule.Intervals = scheduleDto.Intervals
                    .Select(interval => new ModerationShopScheduleInterval
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = schedule.Id,
                        OpenTime = interval.OpenTime,
                        CloseTime = interval.CloseTime
                    })
                    .ToList();
            }

            await dbContext.ModerationShopSchedules.AddAsync(schedule, cancellationToken);
        }
    }
}




