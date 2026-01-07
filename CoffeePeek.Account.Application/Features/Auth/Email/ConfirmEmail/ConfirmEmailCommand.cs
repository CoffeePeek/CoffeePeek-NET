using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.Email.ConfirmEmail;

public record ConfirmEmailCommand(string Token) : IRequest<Response>;