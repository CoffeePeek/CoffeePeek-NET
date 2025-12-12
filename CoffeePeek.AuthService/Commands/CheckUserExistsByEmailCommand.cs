using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.AuthService.Commands;

public record CheckUserExistsByEmailCommand(string Email) : IRequest<Response<bool>>;