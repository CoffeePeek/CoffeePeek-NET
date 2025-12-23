using System.Linq.Expressions;

namespace CoffeePeek.Shared.Infrastructure.Abstract;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
}