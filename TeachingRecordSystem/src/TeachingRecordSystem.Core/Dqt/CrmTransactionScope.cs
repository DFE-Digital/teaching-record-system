using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Dqt;

public sealed class CrmTransactionScope(RequestBuilder requestBuilder, IServiceScope scope) : IDisposable
{
    public void Dispose() => scope.Dispose();

    public Task ExecuteAsync() => requestBuilder.ExecuteAsync();

    public Func<TResult> AppendQuery<TResult>(ICrmTransactionalQuery<TResult> query)
    {
        var handlerType = typeof(ICrmTransactionalQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);

        var wrapperHandlerType = typeof(TransactionalQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var wrappedHandler = (TransactionalQueryHandler<TResult>)Activator.CreateInstance(wrapperHandlerType, handler)!;

        return wrappedHandler.AppendQuery(query, requestBuilder);
    }

    private abstract class TransactionalQueryHandler<T>
    {
        public abstract Func<T> AppendQuery(ICrmTransactionalQuery<T> query, RequestBuilder requestBuilder);
    }

    private class TransactionalQueryHandler<TQuery, TResult>(ICrmTransactionalQueryHandler<TQuery, TResult> innerHandler) : TransactionalQueryHandler<TResult>
        where TQuery : ICrmTransactionalQuery<TResult>
    {
        public override Func<TResult> AppendQuery(ICrmTransactionalQuery<TResult> query, RequestBuilder requestBuilder)
        {
            return innerHandler.AppendQuery((TQuery)query, requestBuilder);
        }
    }
}
