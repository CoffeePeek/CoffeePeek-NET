using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.Account.Application.Features.User.GetProfile;

public record GetPublicUserProfileQuery(Guid UserId) : IRequest<Response<UserProfileResponse>>{}