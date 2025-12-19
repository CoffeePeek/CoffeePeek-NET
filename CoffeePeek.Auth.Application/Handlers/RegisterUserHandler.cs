using CoffeePeek.Auth.Application.Services.Interfaces;
using CoffeePeek.Auth.Application.Services.Validation;
using CoffeePeek.Auth.Domain.Entities;
using CoffeePeek.Auth.Domain.Repositories;
using CoffeePeek.AuthService.Commands;
using CoffeePeek.AuthService.Services;
using CoffeePeek.Contract.Events;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.Shared.Infrastructure.Outbox;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Auth.Application.Handlers;

public class RegisterUserHandler(
    IUserCredentialsRepository credentialsRepo,
    IPasswordHasherService passwordHasher,
    IUserManager userManager,
    IValidationStrategy<RegisterUserCommand> validationStrategy,
    IUnitOfWork unitOfWork,
    IOutboxEventPublisher outboxEventPublisher,
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

            logger.LogDebug("Adding UserRegisteredEvent to outbox for user ID: {UserId}", userAuth.Id);
            await outboxEventPublisher.PublishAsync(userRegisteredEvent, ct);
        
            await unitOfWork.SaveChangesAsync(ct); 
            logger.LogInformation("User and outbox event for {Email} saved successfully in transaction.", request.Email);

            return CreateEntityResponse<Guid>.Success(userAuth.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during user registration for email: {Email}", request.Email);
            return CreateEntityResponse<Guid>.Error(ex.Message);
        }
    }
}