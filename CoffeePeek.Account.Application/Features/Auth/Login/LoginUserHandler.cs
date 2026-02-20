using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Auth.Login;

public class LoginUserHandler
{
    public static async Task<Response<LoginResponse>> Handle(LoginUserCommand request, 
        IAuthService authService,
        EmailExistenceFilter emailExistenceFilter,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var result = await authService.LoginAsync(
            request.Email, 
            request.Password, 
            request.DeviceName, 
            request.IpAddress);

        emailExistenceFilter.Add(request.Email);

        await unitOfWork.SaveChangesAsync(ct);
        
        return Response<LoginResponse>.Success(new LoginResponse(result.AccessToken, result.RefreshToken));
    }
}