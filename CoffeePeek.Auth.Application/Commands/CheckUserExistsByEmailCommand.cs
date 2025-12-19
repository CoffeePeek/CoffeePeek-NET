using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Auth.Application.Commands;

public record CheckUserExistsByEmailCommand(string Email) : IRequest<Response<bool>>;