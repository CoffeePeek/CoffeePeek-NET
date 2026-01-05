using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.JobVacancies.Application.Models.HH;

public class HhVideoVacancy
{
    [Url]
    public string Url { get; set; } = null!;
}