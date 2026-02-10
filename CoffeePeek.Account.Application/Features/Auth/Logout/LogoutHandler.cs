using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.Auth.Logout;

public static class LogoutHandler
{
    [Transactional]
    public static async Task Handle(
        LogoutCommand request, 
        IUserRepository userRepository, 
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct) 
                   ?? throw new NotFoundException("User not found");

        user.Logout(request.RefreshToken);
    }
}