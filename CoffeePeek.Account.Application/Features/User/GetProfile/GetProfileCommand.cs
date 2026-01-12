using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.User;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.GetProfile;

public record GetProfileCommand(Guid UserId) : IRequest<Response<UserProfileResponse>>;
