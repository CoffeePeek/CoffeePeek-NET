using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Responses;
using CoffeePeek.JobVacancies.Entities;
using CoffeePeek.JobVacancies.Models.Responses;
using MediatR;

namespace CoffeePeek.JobVacancies.Commands;

public record GetVacanciesCommand(Guid CityId, CPJobType JobType, [Range(1, int.MaxValue)]int Page, [Range(1, 100)]int PerPage)
    : IRequest<Response<JobVacanciesResponse>>;