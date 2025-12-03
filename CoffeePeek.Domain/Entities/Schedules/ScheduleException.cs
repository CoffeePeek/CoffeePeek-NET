namespace CoffeePeek.Domain.Entities.Schedules;

public class ScheduleException : BaseEntity
{
    public int ShopId { get; set; }
    public Shop.Shop Shop { get; set; }

    public DateTime ExceptionStartDate { get; set; }
    public DateTime ExceptionEndDate { get; set; }
    public DateTime? SpecialOpeningTime { get; set; }
    public DateTime? SpecialClosingTime { get; set; }
    public bool IsSpecialOpen24Hours { get; set; }
    
}