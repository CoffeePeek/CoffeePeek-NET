using CoffeePeek.Account.Application.Common.Interfaces;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Login;
using CoffeePeek.Shared.Infrastructure.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Login;

public class LoginUserHandler(
    IAuthService authService,
    IUnitOfWork unitOfWork)
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

        //await cache.InvalidateUserAsync(request.Email);

        return Response<LoginResponse>.Success(
            new LoginResponse(result.AccessToken, result.RefreshToken));
    }
}