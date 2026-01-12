using System.ComponentModel.DataAnnotations;
using CoffeePeek.Contract.Responses;
using CoffeePeek.JobVacancies.Application.Models.Responses;
using CoffeePeek.JobVacancies.Domain.Entities;
using MediatR;

namespace CoffeePeek.JobVacancies.Application.Commands;

public record GetVacanciesQuery(Guid CityId, CPJobType JobType, [Range(1, int.MaxValue)]int Page, [Range(1, 100)]int PerPage)
    : IRequest<Response<JobVacanciesResponse>>;