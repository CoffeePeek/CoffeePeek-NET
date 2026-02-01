using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest<Response<bool>>;