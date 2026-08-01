namespace CoffeePeek.Account.Application.Features.Admin.Users.Sessions;

public record AdminUserSessionResponse(
    Guid Id,
    string DeviceName,
    string IpAddress,
    DateTime ExpiryDate,
    bool IsRevoked,
    DateTime CreatedAtUtc);
