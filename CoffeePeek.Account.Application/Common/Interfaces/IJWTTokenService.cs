using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Entities;

namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface IJWTTokenService
{
    string GenerateAccessToken(UserCredential user, string device, string ipAddress);
    string GenerateRefreshToken();
}