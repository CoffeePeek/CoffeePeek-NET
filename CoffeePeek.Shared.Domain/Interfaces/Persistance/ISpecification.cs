using System.Linq.Expressions;

namespace CoffeePeek.Shared.Domain.Interfaces.Persistance;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
}