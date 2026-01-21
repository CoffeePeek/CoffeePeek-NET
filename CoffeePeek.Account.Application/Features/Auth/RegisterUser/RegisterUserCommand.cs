using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.RegisterUser;

public class RegisterUserCommand(string userName, string email, string password)
    : IRequest<CreateEntityResponse>
{
    public string UserName { get; } = userName;
    public string Email { get; } = email;
    public string Password { get; } = password;
}