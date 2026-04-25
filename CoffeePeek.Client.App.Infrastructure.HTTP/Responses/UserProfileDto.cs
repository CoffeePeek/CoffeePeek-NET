namespace CoffeePeek.Client.App.Infrastructure.HTTP.Responses;

public sealed class UserProfileDto
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public string? About { get; set; }

    public string? AvatarUrl { get; set; }

    public int ReviewCount { get; set; }

    public int CheckInCount { get; set; }

    public int AddedShopsCount { get; set; }
}
