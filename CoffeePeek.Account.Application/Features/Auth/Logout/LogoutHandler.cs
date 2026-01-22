using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.Logout;

public class LogoutHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken ct)
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