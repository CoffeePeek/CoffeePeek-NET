namespace CoffeePeek.JobVacancies.Application.Models.HH.Responses;

public class HhVacancyResponse
{
    public int Pages { get; set; }
    public List<HhVacancyItem> Items { get; set; } = [];
}