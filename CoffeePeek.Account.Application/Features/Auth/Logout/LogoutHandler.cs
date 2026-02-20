using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Account.Application.Features.Auth.Logout;

public static class LogoutHandler
{
    public static async Task Handle(
        LogoutCommand request, 
        IUserRepository userRepository, 
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct) 
                   ?? throw new NotFoundException("User not found");

        user.Logout(request.RefreshToken);

        await unitOfWork.SaveChangesAsync(ct);
    }
}