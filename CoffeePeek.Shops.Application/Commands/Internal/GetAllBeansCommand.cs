using CoffeePeek.Contract.Response.Internal;
using CoffeePeek.Contract.Responses;
using CoffeePeek.Contract.Responses.Internal;
using MediatR;

namespace CoffeePeek.Contract.Requests.Internal;

public record GetAllBeansCommand : IRequest<Response<GetAllBeansResponse>>;