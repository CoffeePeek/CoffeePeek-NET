using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Contract.Dtos.User;

public class UserDto
{
    [Required] public string UserName { get; set; }
    [Required] public string Email { get; set; }

    public string Token { get; set; }

    public string? About { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AvatarUrl { get; set; }
    public int? ReviewCount { get; set; } = 0;
    public int? CheckInCount { get; set; } = 0;
    public int? AddedShopsCount { get; set; } = 0;
}