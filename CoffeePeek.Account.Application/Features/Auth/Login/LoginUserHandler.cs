using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Auth.Login;

public class LoginUserHandler
{
    public static async Task<Response<LoginResponse>> Handle(LoginUserCommand request, 
        IAuthService authService,
        IUnitOfWork unitOfWork,
        EmailExistenceFilter emailExistenceFilter,
        CancellationToken ct)
    {
        var result = await authService.LoginAsync(
            request.Email, 
            request.Password, 
            request.DeviceName, 
            request.IpAddress);

        await unitOfWork.SaveChangesAsync(ct);

        emailExistenceFilter.Add(request.Email);

        return Response<LoginResponse>.Success(new LoginResponse(result.AccessToken, result.RefreshToken));
    }
}