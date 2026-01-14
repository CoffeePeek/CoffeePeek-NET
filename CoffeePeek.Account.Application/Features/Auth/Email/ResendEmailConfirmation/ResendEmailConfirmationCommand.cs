using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Auth.Email.ResendEmailConfirmation;

public record ResendEmailConfirmationCommand(Guid UserId) : IRequest<Response>;