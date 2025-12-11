using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.JobVacancies.Models;

public class HhVideoVacancy
{
    [Url]
    public string Url { get; set; } = null!;
}