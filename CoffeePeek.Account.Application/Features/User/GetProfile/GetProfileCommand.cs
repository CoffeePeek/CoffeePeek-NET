using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.GetProfile;

public record GetProfileCommand(Guid UserId) : IRequest<Response<UserDto>>;
