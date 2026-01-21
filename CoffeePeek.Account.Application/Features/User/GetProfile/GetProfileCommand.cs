using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.GetProfile;

public record GetProfileCommand(Guid UserId) : IRequest<Response<UserProfileResponse>>;
