using CoffeePeek.Account.Application.Commands;
using CoffeePeek.Account.Application.Services;
using CoffeePeek.Account.Application.Services.Interfaces;
using CoffeePeek.Account.Application.Services.Validation;
using CoffeePeek.Account.Domain.Entities;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using CoffeePeek.User.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Handlers;

public class RegisterUserHandler(
    IUserCredentialsRepository credentialsRepo,
    IUserRepository userRepository,
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

            var userAuth = new UserCredential
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHasher.HashPassword(request.Password),
            };

            logger.LogDebug("Adding new user credentials to the repository for email: {Email}", request.Email);
            await credentialsRepo.AddAsync(userAuth, ct);
            
            await userManager.AddToRoleAsync(userAuth, RoleConsts.User, ct);
            logger.LogInformation("User {Email} successfully added to role {Role}", request.Email, RoleConsts.User);
            
            var user = new Domain.Entities.User
            {
                Id = userAuth.Id,
                Email = request.Email,
                Username = request.UserName
            };
            
            logger.LogDebug("Creating User entity for user ID: {UserId}", userAuth.Id);
            await userRepository.AddAsync(user);
        
            await unitOfWork.SaveChangesAsync(ct); 
            logger.LogInformation("User registration completed successfully for {Email}.", request.Email);

            return CreateEntityResponse<Guid>.Success(userAuth.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during user registration for email: {Email}", request.Email);
            return CreateEntityResponse<Guid>.Error(ex.Message);
        }
    }
}