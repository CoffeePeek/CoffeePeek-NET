using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.UnitOfWork;
using CoffeePeek.Moderation.Application.Requests;
using CoffeePeek.Moderation.Application.Responses;
using CoffeePeek.Moderation.Contract.Abstract;
using CoffeePeek.Moderation.Infrastructure.Services.JWT;
using CoffeePeek.Moderation.Infrastructure.Services.User.Interfaces;
using MediatR;

namespace CoffeePeek.Moderation.Application.Handlers.Auth;

public class LoginRequestHandler(IRepository<User> useRepository, 
    IJWTTokenService jwtTokenService, 
    IUserManager userManager) 
    : IRequestHandler<LoginRequest, Response<LoginResponse>>
{
    public async Task<Response<LoginResponse>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await useRepository.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return Response.ErrorResponse<Response<LoginResponse>>("Account does not exist.");
        }

        var isPasswordValid = userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Response.ErrorResponse<Response<LoginResponse>>("Password is incorrect.");
        }

        var authResult = await jwtTokenService.GenerateTokensAsync(user);
    
        var result = new LoginResponse(authResult.AccessToken, authResult.RefreshToken);

        return Response.SuccessResponse<Response<LoginResponse>>(result);
    }
}