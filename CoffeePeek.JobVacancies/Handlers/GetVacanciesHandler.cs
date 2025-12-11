using CoffeePeek.Contract.Response;
using CoffeePeek.Contract.Responses;
using CoffeePeek.JobVacancies.Commands;
using CoffeePeek.JobVacancies.Models.Dtos;
using CoffeePeek.JobVacancies.Models.Responses;
using CoffeePeek.JobVacancies.Repository;
using CoffeePeek.Shared.Infrastructure.Cache;
using CoffeePeek.Shared.Infrastructure.Interfaces.Redis;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.JobVacancies.Handlers;

public class GetVacanciesHandler(
    IJobVacancyRepository repository,
    IRedisService redisService,
    IMapper mapper)
    : IRequestHandler<GetVacanciesCommand, Response<JobVacanciesResponse>>
{
    public async Task<Response<JobVacanciesResponse>> Handle(GetVacanciesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Page <= 0) request = request with { Page = 1 };
        if (request.PerPage <= 0) request = request with { PerPage = 20 };
        if (request.PerPage > 100) request = request with { PerPage = 100 };

        var cacheKey = CacheKey.Vacancies.GetAll(
            request.CityId,
            (int)request.JobType,
            request.Page,
            request.PerPage);

        var cachedResponse = await redisService.GetAsync<JobVacanciesResponse>(cacheKey);
        if (cachedResponse != null)
        {
            return Response.Success(cachedResponse);
        }

        var items = await repository.GetAllByCityIdWithPagination(request.CityId, request.JobType, request.Page,
                        request.PerPage, cancellationToken);

        var dtoItems = mapper.Map<List<JobVacancyDto>>(items);

        var response = new JobVacanciesResponse
        {
            Items = dtoItems,
            Page = request.Page,
            PerPage = request.PerPage,
            Total = items.Length,
                       TotalPages = (int)Math.Ceiling(items.Length / (double)request.PerPage)
        };

        await redisService.SetAsync(cacheKey, response);

        return Response.Success(response);
    }
}