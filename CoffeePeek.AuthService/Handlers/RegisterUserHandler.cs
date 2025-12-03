using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Configuration;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Models;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.AuthService.Services;
using CoffeePeek.AuthService.Services.Validation;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Response.Auth;
using CoffeePeek.Shared.Infrastructure;
using MassTransit;
using MediatR;
using Response = CoffeePeek.Contract.Response.Response;

namespace CoffeePeek.AuthService.Handlers;

public class RegisterUserHandler(
    IUserCredentialsRepository credentialsRepo,
    IPasswordHasherService passwordHasher,
    IUserManager userManager,
    IPublishEndpoint publishEndpoint,
    IValidationStrategy<RegisterUserCommand> validationStrategy,
    AuthDbContext context)
    : IRequestHandler<RegisterUserCommand, Contract.Response.Response<RegisterUserResponse>>
{
    public async Task<Contract.Response.Response<RegisterUserResponse>> Handle(RegisterUserCommand request,
        CancellationToken ct)
    {
        var validationResult = validationStrategy.Validate(request);

        if (!validationResult.IsValid)
        {
            return Response.ErrorResponse<Contract.Response.Response<RegisterUserResponse>>(validationResult.ErrorMessage);
        }
        
        if (await credentialsRepo.UserExists(request.Email, ct))
        {
            return Response.ErrorResponse<Contract.Response.Response<RegisterUserResponse>>("Email already exists");
        }

        var userAuth = new UserCredentials
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHasher.HashPassword(request.Password),
        };

        await credentialsRepo.AddAsync(userAuth, ct);
        await userManager.AddToRoleAsync(userAuth, RoleConsts.User, ct);

        var userRegisteredEvent = new UserRegisteredEvent(userAuth.Id, request.Email, request.UserName);

        var outboxEvent = new OutboxEvent
        {
            EventType = nameof(UserRegisteredEvent),
            Payload = System.Text.Json.JsonSerializer.Serialize(userRegisteredEvent),
        };

        await credentialsRepo.AddOutboxEventAsync(outboxEvent, ct); 
    
        await context.SaveChangesAsync(ct); 

        return Contract.Response.Response<RegisterUserResponse>.SuccessResponse<Contract.Response.Response<RegisterUserResponse>>();
    }
}