using CoffeePeek.Contract.Dtos.User;

namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface IUserQueries
{
    Task<UserDto?> GetProfileByIdAsync(Guid userId, CancellationToken ct);
}