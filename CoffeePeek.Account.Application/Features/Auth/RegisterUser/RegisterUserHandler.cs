using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace CoffeePeek.Account.Application.Features.Auth.RegisterUser;

public class RegisterUserHandler
{
    [Transactional]
    public async Task<(CreateEntityResponse, UserRegisteredInternalEvent)> Handle(
        RegisterUserCommand request, 
        IQueryUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasherService passwordHasher,
        EmailExistenceFilter emailExistenceFilter,
        ILogger<RegisterUserHandler> logger,
        CancellationToken ct)
    {
        if (emailExistenceFilter.MightExist(request.Email) || !await userRepository.IsEmailUnique(request.Email, ct))
        {
            throw new DomainException("Email already exists");
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);

        var defaultRole = await roleRepository.GetRoleAsync(RoleConsts.User)
                          ?? throw new DomainException("Default role not found");

        var user = Domain.Entities.UserAggregate.User.Register(
            request.Email, 
            request.UserName,
            passwordHash,
            defaultRole);

        userRepository.Add(user, ct);

        emailExistenceFilter.Add(request.Email);
        logger.LogInformation("User {Email} registered with ID {UserId}", request.Email, user.Id);

        var response = CreateEntityResponse.Success();
        
        var @event = new UserRegisteredInternalEvent(
            user.Id,
            user.Credentials.Email.Value, 
            user.Username.Value, 
            user.Credentials.EmailConfirmationToken!);

        return (response, @event);
    }
}