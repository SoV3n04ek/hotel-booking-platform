using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace HotelBooking.Application.Tests.Infrastructure.EfHelpers;

public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        // Identify the element type (e.g., Hotel)
        var elementType = expression.Type.GetGenericArguments().FirstOrDefault()
                          ?? typeof(TEntity);

        // Create the correct TestAsyncEnumerable<T> for that type
        var enumerableType = typeof(TestAsyncEnumerable<>).MakeGenericType(elementType);

        // Let the inner provider handle the expression logic, 
        // then wrap the result back in our Async wrapper.
        return (IQueryable)Activator.CreateInstance(
            enumerableType,
            _inner.CreateQuery(expression))!;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(_inner.CreateQuery<TElement>(expression));
    }

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = _inner.Execute(expression);

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

public class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IOrderedQueryable<T>
{
    private readonly IQueryable<T> _inner;

    // Standard constructor for the initial list
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
    {
        _inner = enumerable.AsQueryable();
    }

    // Constructor for chained queries (OrderBy, Where, etc.)
    public TestAsyncEnumerable(IQueryable<T> inner)
    {
        _inner = inner;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(_inner.GetEnumerator());

    public Type ElementType => _inner.ElementType;
    public Expression Expression => _inner.Expression;

    // Wrap the provider to ensure the next call in the chain is also Async-compatible
    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner.Provider);

    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _inner.GetEnumerator();
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    public T Current => _inner.Current;
}