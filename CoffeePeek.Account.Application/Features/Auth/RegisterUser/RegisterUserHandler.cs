using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Shared.Auth.Constants;
using CoffeePeek.Shared.Kernel;
using CoffeePeek.Shared.Kernel.Exceptions;
using CoffeePeek.Shared.Kernel.Response;

namespace CoffeePeek.Account.Application.Features.Auth.RegisterUser;

public static class RegisterUserHandler
{
    public static async Task<(CreateEntityResponse, UserRegisteredInternalEvent)> Handle(
        RegisterUserCommand request,
        IQueryUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasherService passwordHasher,
        EmailExistenceFilter emailExistenceFilter,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (emailExistenceFilter.MightExist(request.Email) || !await userRepository.IsEmailUnique(request.Email, ct))
            throw new DomainException("Email already exists");

        if (request.Password.Length < 8)
            throw new DomainException("Password must be at least 8 characters long");

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

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConflictException)
        {
            // Race condition: two concurrent registrations with the same email.
            throw new DomainException("Email already exists");
        }

        var registeredEvent = new UserRegisteredInternalEvent(
            user.Id,
            user.Credentials.Email.Value,
            user.Username.Value,
            user.Credentials.EmailConfirmationToken!);

        return (CreateEntityResponse.Success(), registeredEvent);
    }
}