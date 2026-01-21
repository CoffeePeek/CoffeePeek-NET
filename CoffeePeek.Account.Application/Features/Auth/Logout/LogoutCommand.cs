using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.Logout;

public record LogoutCommand(Guid UserId, string RefreshToken) : IRequest<Response>;
