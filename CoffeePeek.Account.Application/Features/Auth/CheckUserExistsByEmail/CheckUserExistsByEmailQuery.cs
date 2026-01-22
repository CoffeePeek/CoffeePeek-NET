using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.CheckUserExistsByEmail;

public record CheckUserExistsByEmailQuery(string Email) : IRequest<Response<bool>>;