using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.UpdateEmail;

public record UpdateEmailCommand(Guid UserId, string Email) : IRequest<UpdateEntityResponse<string>>;