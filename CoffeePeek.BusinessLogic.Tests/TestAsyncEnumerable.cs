using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace CoffeePeek.BusinessLogic.Tests;

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression) : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

public class TestAsyncEnumerator<T>(IEnumerator<T> enumerator) : IAsyncEnumerator<T>
{
    public ValueTask DisposeAsync()
    {
        enumerator.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(enumerator.MoveNext());
    public T Current => enumerator.Current;
}

public class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        new TestAsyncEnumerable<TElement>(expression);

    public object Execute(Expression expression) => inner.Execute(expression);
    public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        return Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) =>
        new TestAsyncEnumerable<TResult>(expression as Expression);
}