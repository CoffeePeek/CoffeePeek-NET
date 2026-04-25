using System.Linq;
using CoffeePeek.Contract.Dtos.Schedule;

namespace CoffeePeek.Client.App.ViewModels.Shops;

public sealed class ScheduleLineViewModel
{
    public string DayName { get; init; } = string.Empty;
    public string Hours { get; init; } = string.Empty;
    public bool IsClosed { get; init; }

    public static ScheduleLineViewModel From(ScheduleDto dto)
    {
        var dayName = dto.DayOfWeek switch
        {
            DayOfWeek.Monday => "Mon",
            DayOfWeek.Tuesday => "Tue",
            DayOfWeek.Wednesday => "Wed",
            DayOfWeek.Thursday => "Thu",
            DayOfWeek.Friday => "Fri",
            DayOfWeek.Saturday => "Sat",
            DayOfWeek.Sunday => "Sun",
            _ => dto.DayOfWeek.ToString()
        };

        if (dto.IsClosed)
            return new ScheduleLineViewModel { DayName = dayName, Hours = "Closed", IsClosed = true };

        var intervals = dto.Intervals?
            .Select(i => $"{i.OpenTime:hh\\:mm}–{i.CloseTime:hh\\:mm}")
            .ToArray() ?? [];

        return new ScheduleLineViewModel
        {
            DayName = dayName,
            Hours = intervals.Length > 0 ? string.Join(", ", intervals) : "—"
        };
    }
}
