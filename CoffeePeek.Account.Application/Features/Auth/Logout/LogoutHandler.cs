using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Exceptions;

namespace CoffeePeek.Account.Application.Features.Auth.Logout;

public class LogoutHandler
{
    public async Task Handle(LogoutCommand request, 
        IUserRepository userRepository,
        IUnitOfWork unitOfWork, 
        CancellationToken ct)
    {
        var user = await userRepository.GetById(request.UserId, ct);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.Logout(request.RefreshToken);

        await unitOfWork.SaveChangesAsync(ct);
    }
}