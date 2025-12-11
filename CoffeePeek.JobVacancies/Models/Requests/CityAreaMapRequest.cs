using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.JobVacancies.Models.Requests;

public record CityAreaMapRequest(
    [property: Required] Guid CityId,
    [property: Range(1, int.MaxValue)] int HhAreaId);


