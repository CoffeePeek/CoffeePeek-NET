using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.Auth;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Data;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Domain.UnitOfWork;
using CoffeePeek.Infrastructure.Cache.Interfaces;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CoffeePeek.BusinessLogic.RequestHandlers;

public class RegisterUserRequestHandler(
    IRepository<User> userRepository,
    IMapper mapper,
    IValidationStrategy<UserDto> validationStrategy,
    UserManager<User> userManager,
    IRedisService redisService)
    : IRequestHandler<RegisterUserRequest, Response<RegisterUserResponse>>
{
    public async Task<Response<RegisterUserResponse>> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var userExists = await userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
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
        
        var createUserResult = await userManager.CreateAsync(user, request.Password);

        if (!createUserResult.Succeeded)
        {
            return Response.ErrorResponse<Response<RegisterUserResponse>>(createUserResult.ToString());
        }
        
        //if (request.IsAdmin)
        //{
        //    var addRoleResult = await userManager.AddToRoleAsync(createdUser, RoleConsts.Admin);
        //}
        //else
        //{
        //    var addRoleResult = await userManager.AddToRoleAsync(createdUser, RoleConsts.User);
        //}
        
        await userRepository.SaveChangesAsync(cancellationToken);
        
        var createdUser = await userManager.FindByEmailAsync(request.Email);
        
        await redisService.SetAsync($"{nameof(User)}{createdUser!.Id}", createdUser);

        var result = mapper.Map<RegisterUserResponse>(createdUser);
        
        return Response.SuccessResponse<Response<RegisterUserResponse>>(result);
    }
}