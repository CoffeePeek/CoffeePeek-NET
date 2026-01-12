using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Responses;
using CoffeePeek.JobVacancies.Application.Commands;
using CoffeePeek.JobVacancies.Application.Models.Responses;
using CoffeePeek.JobVacancies.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.JobVacancies.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VacanciesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<Response<JobVacanciesResponse>> GetJobs(
        [FromQuery, Required] Guid cityId,
        [FromQuery] CPJobType jobType = CPJobType.All,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int perPage = 20,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new GetVacanciesQuery(cityId, jobType, page, perPage), cancellationToken);
    }
}