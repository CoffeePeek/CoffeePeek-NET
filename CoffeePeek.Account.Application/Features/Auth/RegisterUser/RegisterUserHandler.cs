using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Features.Auth.RegisterUser;

public class RegisterUserHandler
{
    public static async Task<CreateEntityResponse> Handle(
        RegisterUserCommand request, 
        IQueryUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasherService passwordHasher,
        IUnitOfWork unitOfWork,
        IEventPublisher publishEndpoint,
        EmailExistenceFilter emailExistenceFilter,
        ILogger<RegisterUserHandler> logger,
        CancellationToken ct)
    {
        if (emailExistenceFilter.MightExist(request.Email) || !await userRepository.IsEmailUnique(request.Email, ct))
            throw new DomainException("Email already exists");

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

        await publishEndpoint.Publish(new UserRegisteredInternalEvent(
            user.Id,
            user.Credentials.Email.Value, 
            user.Username.Value, 
            user.Credentials.EmailConfirmationToken!), ct);

        await unitOfWork.SaveChangesAsync(ct);

        return CreateEntityResponse.Success();
    }
}