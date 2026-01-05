namespace CoffeePeek.JobVacancies.Application.Models.HH.Responses;

public class HhAreaNode
{
    public string? Id { get; set; }
    public string Name { get; set; } = null!;
    public List<HhAreaNode>? Areas { get; set; }
}

