namespace CoffeePeek.JobVacancies.Models.Responses;

public class HhVacancyResponse
{
    public int Pages { get; set; }
    public List<HhVacancyItem> Items { get; set; } = [];
}