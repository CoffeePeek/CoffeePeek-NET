using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoffeePeek.Shops.Application.Features.CheckIn.GetUserCheckIns;

public record GetUserCheckInsCommand(
    [property: JsonIgnore] Guid UserId,
    [Range(1, int.MaxValue)] int PageNumber,
    [Range(1, 100)] int PageSize);
