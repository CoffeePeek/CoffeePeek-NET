using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.RegisterUser;

public record RegisterUserCommand(string UserName, string Email, string Password) : IRequest<CreateEntityResponse>;