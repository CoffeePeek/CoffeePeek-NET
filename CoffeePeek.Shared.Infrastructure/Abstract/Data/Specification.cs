using System.Linq.Expressions;

namespace CoffeePeek.Shared.Infrastructure.Abstract;

public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; protected init; } = null!;
}