namespace CoffeePeek.Web.Contract.Http.Shops;

public class ScheduleDto
{
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
    public bool IsOpen24Hours { get; set; }
}