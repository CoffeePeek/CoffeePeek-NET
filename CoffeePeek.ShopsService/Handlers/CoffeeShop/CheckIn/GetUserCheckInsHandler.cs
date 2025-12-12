using CoffeePeek.Contract.Dtos.CoffeeShop;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Response.CoffeeShop;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.CoffeeShop;
using CoffeePeek.ShopsService.DB;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.ShopsService.Handlers.CoffeeShop.CheckIn;

public class GetUserCheckInsHandler(
    ShopsDbContext dbContext,
    IMapper mapper)
    : IRequestHandler<GetUserCheckInsCommand, Response<GetUserCheckInsResponse>>
{
    public async Task<Response<GetUserCheckInsResponse>> Handle(GetUserCheckInsCommand request, CancellationToken cancellationToken)
    {
        var query = dbContext.CheckIns
            .AsNoTracking()
            .Include(c => c.Shop)
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var checkIns = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var checkInDtos = mapper.Map<CheckInDto[]>(checkIns);

        var response = new GetUserCheckInsResponse(
            checkIns: checkInDtos,
            totalItems: totalItems,
            totalPages: totalPages,
            currentPage: request.PageNumber,
            pageSize: request.PageSize);

        return Response<GetUserCheckInsResponse>.Success(response);
    }
}
