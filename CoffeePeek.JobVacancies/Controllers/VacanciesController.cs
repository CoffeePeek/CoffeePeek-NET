using CoffeePeek.Contract.Responses;
using CoffeePeek.JobVacancies.Commands;
using CoffeePeek.JobVacancies.Entities;
using CoffeePeek.JobVacancies.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoffeePeek.JobVacancies.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VacanciesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<Response<JobVacanciesResponse>> GetJobs(
        [FromQuery] Guid cityId,
        [FromQuery] CPJobType jobType = CPJobType.All,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20,
        CancellationToken cancellationToken = default)
    {
        return await mediator.Send(new GetVacanciesCommand(cityId, jobType, page, perPage), cancellationToken);
    }
}