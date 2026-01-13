using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Contract.Dtos.User;

public class UserDto
{
    [Required] public string UserName { get; init; }
    [Required] public string Email { get; init; }

    public string Token { get; init; }

    public string? About { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? AvatarUrl { get; init; }
    public int? ReviewCount { get; init; } = 0;
    public int? CheckInCount { get; init; } = 0;
    public int? AddedShopsCount { get; init; } = 0;
}