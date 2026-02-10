using System.Linq.Expressions;
using CoffeePeek.Shared.Domain.Interfaces.Persistance;

namespace CoffeePeek.Shared.Persistence;

public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; protected init; } = null!;
}