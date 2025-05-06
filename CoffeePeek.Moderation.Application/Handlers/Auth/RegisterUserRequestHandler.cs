using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Moderation.Application.Requests;
using CoffeePeek.Moderation.Application.Responses;
using CoffeePeek.Moderation.Application.Services;
using CoffeePeek.Moderation.Contract.Abstract;
using CoffeePeek.Moderation.Contract.DTOs;
using CoffeePeek.Moderation.Infrastructure.Services.User.Interfaces;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.Moderation.Application.Handlers.Auth;

internal class RegisterUserRequestHandler(
    IUserManager userManager,
    IMapper mapper,
    ValidationStrategy validationStrategy)
    : IRequestHandler<RegisterUserRequest, Response<RegisterUserResponse>>
{
    public async Task<Response<RegisterUserResponse>> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Response.ErrorResponse<Response<RegisterUserResponse>>("Email already exists");
        }

        var userDto = mapper.Map<UserDto>(request);
        var validationResult = validationStrategy.ValidateUserRegister(userDto.Email, userDto.Password);

        if (!validationResult)
        {
            return Response.ErrorResponse<Response<RegisterUserResponse>>($"Invalid request: ");
        }

        var user = mapper.Map<User>(userDto);

        if (!await TryCreateUserAsync(user, request.Password))
        {
            return Response.ErrorResponse<Response<RegisterUserResponse>>("Cannot create user");
        }

        var createdUser = await userManager.FindByEmailAsync(request.Email);

        if (createdUser is null)
        {
            return Response.ErrorResponse<Response<RegisterUserResponse>>("Cannot create user");
        }
        
        var response = mapper.Map<RegisterUserResponse>(createdUser);
        
        return Response.SuccessResponse<Response<RegisterUserResponse>>(response);
    }

    private async Task<bool> TryCreateUserAsync(User user, string password)
    {
        try
        {
            await userManager.CreateAsync(user, password);
            return true;
        }
        catch
        {
            return false;
        }
    }
}