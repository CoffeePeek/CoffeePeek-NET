using CoffeePeek.Account.Domain.Aggregates.UserAggregate;

namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface IJWTTokenService
{
    string GenerateAccessToken(UserCredential user, string device, string ipAddress);
    string GenerateRefreshToken();
}