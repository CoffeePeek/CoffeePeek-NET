using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.AuthService.Services;
using CoffeePeek.AuthService.Services.Validation;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Response;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure;
using MediatR;

namespace CoffeePeek.AuthService.Handlers;

public class RegisterUserHandler(
    IUserCredentialsRepository credentialsRepo,
    IPasswordHasherService passwordHasher,
    IUserManager userManager,
    IValidationStrategy<RegisterUserCommand> validationStrategy,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterUserCommand, CreateEntityResponse<Guid>>
{
    public async Task<CreateEntityResponse<Guid>> Handle(RegisterUserCommand request,
        CancellationToken ct)
    {
        var validationResult = validationStrategy.Validate(request);

        if (!validationResult.IsValid)
        {
            return CreateEntityResponse<Guid>.Error(validationResult.ErrorMessage);
        }
        
        if (await credentialsRepo.UserExists(request.Email, ct))
        {
            return CreateEntityResponse<Guid>.Error("Email already exists");
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
    
        await unitOfWork.SaveChangesAsync(ct); 

        return CreateEntityResponse<Guid>.Success(userAuth.Id);
    }
}