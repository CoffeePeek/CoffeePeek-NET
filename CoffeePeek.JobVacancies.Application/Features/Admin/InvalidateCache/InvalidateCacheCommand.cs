using CoffeePeek.Contract.Abstract;
using CoffeePeek.Contract.Responses;
using MediatR;

namespace CoffeePeek.JobVacancies.Application.Features.Admin.InvalidateCache;

public record InvalidateCacheCommand(
    bool InvalidateAll = true) : IRequest<Response<InvalidateCacheResponse>>;
