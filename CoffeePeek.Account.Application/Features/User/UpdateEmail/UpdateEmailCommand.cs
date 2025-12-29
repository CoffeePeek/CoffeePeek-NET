using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.UpdateEmail;

public record UpdateEmailCommand(Guid UserId, string Email) : IRequest<UpdateEntityResponse<string>>;