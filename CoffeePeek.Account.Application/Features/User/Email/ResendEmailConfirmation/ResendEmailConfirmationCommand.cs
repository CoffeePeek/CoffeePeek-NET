using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.Email.ResendEmailConfirmation;

public record ResendEmailConfirmationCommand(Guid UserId) : IRequest<Response>;