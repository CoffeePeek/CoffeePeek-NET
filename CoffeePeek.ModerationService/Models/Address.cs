using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.ModerationService.Models;

public class Address
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CityId { get; set; }
    public int StreetId { get; set; }
    [MaxLength(70)]
    public string BuildingNumber { get; set; }
    [MaxLength(15)]
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

