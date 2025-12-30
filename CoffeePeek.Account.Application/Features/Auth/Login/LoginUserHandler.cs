using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Account.Application.Features.Login;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Login;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.Login;

public class LoginUserHandler(
    IAuthService authService,
    IUnitOfWork unitOfWork,
    EmailExistenceFilter emailExistenceFilter)
    : IRequestHandler<LoginUserCommand, Response<LoginResponse>>
{
    public async Task<Response<LoginResponse>> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(
            request.Email, 
            request.Password, 
            request.DeviceName, 
            request.IpAddress);

        await unitOfWork.SaveChangesAsync(ct);

        emailExistenceFilter.Add(request.Email);
        //await cache.InvalidateUserAsync(request.Email);

        return Response<LoginResponse>.Success(
            new LoginResponse(result.AccessToken, result.RefreshToken));
    }
}