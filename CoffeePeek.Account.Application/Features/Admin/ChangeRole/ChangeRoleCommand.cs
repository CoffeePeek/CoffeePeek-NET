using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Admin.ChangeRole;

public record ChangeRoleCommand(Guid UserId, Guid UserIdOfChange, Guid RoleId) : IRequest<Response>;