using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ConfirmEmail;

public record ConfirmEmailCommand(string Token) : IRequest<Response>;