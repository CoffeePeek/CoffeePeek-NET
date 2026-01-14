using CoffeePeek.Contract.Abstract;
using MediatR;

namespace Coffeepeek.Moderation.Application.Features.Shop.GetAllModerationShops;

public class GetAllModerationShopsQuery : IRequest<Response<GetAllModerationShopsResponse>>;

