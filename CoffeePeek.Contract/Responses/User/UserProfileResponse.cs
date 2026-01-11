using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Contract.Responses.User;

public class UserProfileResponse
{
    [Required] public string UserName { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public string Token { get; set; }

    public string? About { get; set; }
    public string? AvatarUrl { get; set; }
    public int? ReviewCount { get; set; } = 0;
    public int? CheckInCount { get; set; } = 0;
    public int? AddedShopsCount { get; set; } = 0;
}