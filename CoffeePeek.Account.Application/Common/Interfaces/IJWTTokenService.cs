using CoffeePeek.Account.Domain.Entities.UserAggregate;

namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface IJWTTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}