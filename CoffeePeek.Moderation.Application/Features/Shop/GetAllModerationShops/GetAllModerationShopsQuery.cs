using CoffeePeek.Contract.Abstract;
using MediatR;

namespace CoffeePeek.Moderation.Application.Features.Shop.GetAllModerationShops;

public class GetAllModerationShopsQuery : IRequest<Response<GetAllModerationShopsResponse>>;

