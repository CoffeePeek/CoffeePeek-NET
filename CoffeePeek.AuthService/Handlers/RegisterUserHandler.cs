using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Entities;
using CoffeePeek.AuthService.Repositories;
using CoffeePeek.AuthService.Services;
using CoffeePeek.AuthService.Services.Validation;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Data.Interfaces;
using CoffeePeek.Shared.Infrastructure;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;

namespace CoffeePeek.AuthService.Handlers;

public class RegisterUserHandler(
    IUserCredentialsRepository credentialsRepo,
    IPasswordHasherService passwordHasher,
    IUserManager userManager,
    IValidationStrategy<RegisterUserCommand> validationStrategy,
    IUnitOfWork unitOfWork,
    ILogger<RegisterUserHandler> logger)
    : IRequestHandler<RegisterUserCommand, CreateEntityResponse<Guid>>
{
    public async Task<CreateEntityResponse<Guid>> Handle(RegisterUserCommand request,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Attempting to register user with email: {Email}", request.Email);
            var validationResult = validationStrategy.Validate(request);

            if (!validationResult.IsValid)
            {
                logger.LogWarning("User registration failed due to validation errors for email: {Email}. Errors: {Errors}",
                    request.Email, validationResult.ErrorMessage);
                return CreateEntityResponse<Guid>.Error(validationResult.ErrorMessage);
            }
            
            if (await credentialsRepo.UserExists(request.Email, ct))
            {
                return CreateEntityResponse<Guid>.Error("Email already exists");
            }
            logger.LogInformation("User with email {Email} does not exist. Proceeding with registration.", request.Email);

            var userAuth = new UserCredentials
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHasher.HashPassword(request.Password),
            };

            logger.LogDebug("Adding new user credentials to the repository for email: {Email}", request.Email);
            await credentialsRepo.AddAsync(userAuth, ct);
                
            await unitOfWork.SaveChangesAsync(ct);
            logger.LogInformation("User credentials saved for email: {Email}", request.Email);
            
            await userManager.AddToRoleAsync(userAuth, RoleConsts.User, ct);
            logger.LogInformation("User {Email} successfully added to role {Role}", request.Email, RoleConsts.User);
            
            var userRegisteredEvent = new UserRegisteredEvent(userAuth.Id, request.Email, request.UserName);

            var outboxEvent = new OutboxEvent
            {
                EventType = nameof(UserRegisteredEvent),
                Payload = System.Text.Json.JsonSerializer.Serialize(userRegisteredEvent),
            };

            logger.LogDebug("Adding UserRegisteredEvent to outbox for user ID: {UserId}", userAuth.Id);
            await credentialsRepo.AddOutboxEventAsync(outboxEvent, ct); 
        
            await unitOfWork.SaveChangesAsync(ct); 
            logger.LogInformation("Outbox event for user {Email} saved successfully.", request.Email);

            return CreateEntityResponse<Guid>.Success(userAuth.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during user registration for email: {Email}", request.Email);
            return CreateEntityResponse<Guid>.Error(ex.Message);
        }
    }
}