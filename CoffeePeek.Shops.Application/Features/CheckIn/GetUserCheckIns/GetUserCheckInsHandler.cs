using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shops.Domain.Aggregates.CoffeeShopAggregate;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.Shops.Application.Features.CheckIn.GetUserCheckIns;

public class GetUserCheckInsHandler(
    IGenericRepository<Domain.Aggregates.CheckInAggregate.CheckIn> checkInRepository,
    ICoffeeShopRepository coffeeShopRepository,
    IMapper mapper)
    : IRequestHandler<GetUserCheckInsCommand, Response<GetUserCheckInsResponse>>
{
    public async Task<Response<GetUserCheckInsResponse>> Handle(GetUserCheckInsCommand request, CancellationToken cancellationToken)
    {
        var query = checkInRepository.QueryAsNoTracking()
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.CreatedAtUtc);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var checkIns = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Get shop names for all check-ins in batch
        var shopIds = checkIns.Select(c => c.ShopId).Distinct().ToList();
        var shopNames = await coffeeShopRepository.GetShopNamesByIdsAsync(shopIds, cancellationToken);

        var checkInDtos = checkIns.Select(c =>
        {
            var dto = mapper.Map<CheckInDto>(c);
            dto.ShopName = shopNames.GetValueOrDefault(c.ShopId, string.Empty);
            return dto;
        }).ToArray();

        var response = new GetUserCheckInsResponse(
            checkInDtos,
            totalItems,
            totalPages,
            request.PageNumber,
            request.PageSize);

        return Response<GetUserCheckInsResponse>.Success(response);
    }
}