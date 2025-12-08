using CoffeePeek.Contract.Response.Internal;
using MediatR;

namespace CoffeePeek.Contract.Requests.Internal;

public record GetAllBeansCommand : IRequest<Response.Response<GetAllBeansResponse>>;