using CoffeePeek.BuildingBlocks.AuthOptions;
using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class RegisterUserRequestHandler(
    IMapper mapper,
    IValidationStrategy<UserDto> validationStrategy,
    UserManager<User> userManager,
    RoleManager<IdentityRole<int>> roleManager,
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
        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Response.ErrorResponse<Response<RegisterUserResponse>>($"Cannot create user: {errors}");
        }

        var defaultRoles = new[] { RoleConsts.User };
        foreach (var roleName in defaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
            }
        }

        await userManager.AddToRoleAsync(user, RoleConsts.User);

        await redisService.SetAsync($"{nameof(User)}{user.Id}", user);

        var response = mapper.Map<RegisterUserResponse>(user);
        return Response.SuccessResponse<Response<RegisterUserResponse>>(response);
    }
}