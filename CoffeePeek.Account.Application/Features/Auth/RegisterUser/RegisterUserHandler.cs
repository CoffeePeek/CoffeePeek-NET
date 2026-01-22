using CoffeePeek.Account.Application.Common;
using CoffeePeek.Account.Domain.Entities.RoleAggregate;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
using CoffeePeek.Account.Domain.Services;
using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Shared.Extensions.Exceptions;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoffeePeek.Account.Application.Features.Auth.RegisterUser;

public class RegisterUserHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasherService passwordHasher,
    IUnitOfWork unitOfWork,
    EmailExistenceFilter emailExistenceFilter,
    ILogger<RegisterUserHandler> logger)
    : IRequestHandler<RegisterUserCommand, CreateEntityResponse>
{
    public async Task<CreateEntityResponse> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        if (emailExistenceFilter.MightExist(request.Email) || !await userRepository.IsEmailUnique(request.Email, ct))
        {
            throw new DomainException("Email already exists");
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);
        
        var defaultRole = await roleRepository.GetRoleAsync(RoleConsts.User)
                          ?? throw new DomainException("Default role not found");

        var user = Domain.Entities.UserAggregate.User.Register(request.Email, invalidUsername:request.UserName, passwordHash,
            defaultRole);

        await userRepository.Add(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        emailExistenceFilter.Add(request.Email);

        logger.LogInformation("User {Email} registered with ID {UserId}", request.Email, user.Id);
        return CreateEntityResponse.Success();
    }
}