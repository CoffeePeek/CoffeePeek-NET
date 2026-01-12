using CoffeePeek.Contract.Responses;
using CoffeePeek.JobVacancies.Application.Commands;
using CoffeePeek.JobVacancies.Application.Models.Dtos;
using CoffeePeek.JobVacancies.Application.Models.Responses;
using CoffeePeek.JobVacancies.Domain.Repositories;
using CoffeePeek.Shared.Infrastructure.Abstract;
using CoffeePeek.Shared.Infrastructure.Cache;
using MapsterMapper;
using MediatR;

namespace CoffeePeek.JobVacancies.Application.Handlers;

public class GetVacanciesHandler(
    IJobVacancyRepository repository,
    IRedisService redisService,
    IMapper mapper)
    : IRequestHandler<GetVacanciesQuery, Response<JobVacanciesResponse>>
{
    public async Task<Response<JobVacanciesResponse>> Handle(GetVacanciesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page <= 0) request = request with { Page = 1 };
        if (request.PerPage <= 0) request = request with { PerPage = 20 };
        if (request.PerPage > 100) request = request with { PerPage = 100 };

        var cacheKey = CacheKey.Vacancy.List(
            request.CityId,
            (int)request.JobType,
            request.Page,
            request.PerPage);

        var response = await redisService.GetAsync(
            cacheKey,
            async () =>
            {
                var items = await repository.GetAllByCityIdWithPagination(
                    request.CityId, 
                    request.JobType, 
                    request.Page,
                    request.PerPage, 
                    cancellationToken);

                var dtoItems = mapper.Map<List<JobVacancyDto>>(items);

                return new JobVacanciesResponse
                {
                    Items = dtoItems,
                    Page = request.Page,
                    PerPage = request.PerPage,
                    Total = items.Length,
                    TotalPages = (int)Math.Ceiling(items.Length / (double)request.PerPage)
                };
            },
            cacheKey.DefaultTtl);

        return response != null 
            ? Response.Success(response) 
            : Response<JobVacanciesResponse>.Error("Failed to retrieve vacancies");
    }
}