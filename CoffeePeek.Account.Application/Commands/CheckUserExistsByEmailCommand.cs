using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Commands;

public record CheckUserExistsByEmailCommand(string Email) : IRequest<Response<bool>>;