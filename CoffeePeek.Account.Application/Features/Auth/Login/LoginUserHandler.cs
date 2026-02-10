using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;
using CoffeePeek.Shared.Kernel.Response;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.Auth.Login;

public class LoginUserHandler
{
    [Transactional]
    public static async Task<Response<LoginResponse>> Handle(LoginUserCommand request, 
        IAuthService authService,
        EmailExistenceFilter emailExistenceFilter,
        CancellationToken ct)
    {
        var result = await authService.LoginAsync(
            request.Email, 
            request.Password, 
            request.DeviceName, 
            request.IpAddress);

        emailExistenceFilter.Add(request.Email);

        return Response<LoginResponse>.Success(new LoginResponse(result.AccessToken, result.RefreshToken));
    }
}