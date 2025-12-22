using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Commands;

public class RegisterUserCommand(string userName, string email, string password)
    : IRequest<CreateEntityResponse<Guid>>
{
    public string UserName { get; } = userName;
    public string Email { get; } = email;
    public string Password { get; } = password;
}