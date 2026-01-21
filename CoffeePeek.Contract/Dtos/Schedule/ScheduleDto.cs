#nullable enable
namespace CoffeePeek.Contract.Dtos.Schedule;

public record ScheduleDto(DayOfWeek DayOfWeek, bool IsClosed, List<ShopScheduleIntervalDto>? Intervals);
