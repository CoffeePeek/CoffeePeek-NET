using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.CheckUserExistsByEmail;

public record CheckUserExistsByEmailCommand(string Email) : IRequest<Response<bool>>;