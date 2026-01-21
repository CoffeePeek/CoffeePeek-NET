using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.CheckUserExistsByEmail;

public record CheckUserExistsByEmailCommand(string Email) : IRequest<Response<bool>>;