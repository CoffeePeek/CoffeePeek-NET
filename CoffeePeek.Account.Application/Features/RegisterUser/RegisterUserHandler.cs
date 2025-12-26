using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Application.Features.CheckUserExistsByEmail;
using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Account.Domain.Repositories;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Features.RegisterUser;

public class RegisterUserHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasherService passwordHasher,
    IUnitOfWork unitOfWork,
    EmailExistenceFilter emailExistenceFilter,
    ILogger<RegisterUserHandler> logger)
    : IRequestHandler<RegisterUserCommand, CreateEntityResponse<Guid>>
{
    public async Task<CreateEntityResponse<Guid>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        try
        {
            if (emailExistenceFilter.MightExist(request.Email) || !await userRepository.IsEmailUnique(request.Email, ct))
            {
                return CreateEntityResponse<Guid>.Error("Email already exists");
            }

            var passwordHash = passwordHasher.HashPassword(request.Password);
            var user = User.Create(request.Email, request.UserName, passwordHash);

            var defaultRole = await roleRepository.GetRoleAsync(RoleConsts.User)
                              ?? throw new DomainException("Default role not found");
            
            user.AssignRole(defaultRole);

            await userRepository.Add(user, ct);
            await unitOfWork.SaveChangesAsync(ct);

            emailExistenceFilter.Add(request.Email);

            logger.LogInformation("User {Email} registered with ID {UserId}", user.Email, user.Id);
            return CreateEntityResponse<Guid>.Success(user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Registration failed for {Email}", request.Email);
            return CreateEntityResponse<Guid>.Error("An internal error occurred during registration.");
        }
    }
}