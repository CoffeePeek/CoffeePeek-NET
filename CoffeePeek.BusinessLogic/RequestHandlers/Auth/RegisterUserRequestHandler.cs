using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using CoffeePeek.Infrastructure.Services.User.Interfaces;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class RegisterUserRequestHandler(
    IMapper mapper,
    IValidationStrategy<UserDto> validationStrategy,
    IUserManager userManager,
    IRedisService redisService)
    : IRequestHandler<RegisterUserRequest, Response<RegisterUserResponse>>
{
    public async Task<Response<RegisterUserResponse>> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Response.ErrorResponse<Response<RegisterUserResponse>>("Email already exists");
        }

        var userDto = mapper.Map<UserDto>(request);
        var validationResult = validationStrategy.Validate(userDto);

        if (!validationResult.IsValid)
        {
            return Response.ErrorResponse<Response<RegisterUserResponse>>($"Invalid request: {validationResult.ErrorMessage}");
        }

        var user = mapper.Map<User>(userDto);

        if (!await TryCreateUserAsync(user, request.Password))
        {
            return Response.ErrorResponse<Response<RegisterUserResponse>>("Cannot create user");
        }

        var createdUser = await userManager.FindByEmailAsync(request.Email);
        await redisService.SetAsync($"{nameof(User)}{createdUser!.Id}", createdUser);

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