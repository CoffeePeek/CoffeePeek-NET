using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest<Response<bool>>;
