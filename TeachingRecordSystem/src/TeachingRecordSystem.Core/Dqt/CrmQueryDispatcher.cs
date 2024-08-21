using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.Core.Dqt;

public class CrmQueryDispatcher(IServiceProvider serviceProvider, string? serviceClientName) : ICrmQueryDispatcher
{
    public async Task<TResult> ExecuteQuery<TResult>(ICrmQuery<TResult> query)
    {
        using var scope = serviceProvider.CreateScope();

        var organizationService = scope.ServiceProvider.GetRequiredKeyedService<IOrganizationServiceAsync>(serviceClientName);

        var handlerType = typeof(ICrmQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);

        var wrapperHandlerType = typeof(QueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var wrappedHandler = (QueryHandler<TResult>)Activator.CreateInstance(wrapperHandlerType, handler)!;

        return await wrappedHandler.Execute(query, organizationService);
    }

    public async IAsyncEnumerable<TResult> ExecuteQuery<TResult>(IEnumerableCrmQuery<TResult> query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var organizationService = scope.ServiceProvider.GetRequiredKeyedService<IOrganizationServiceAsync>(serviceClientName);

        var handlerType = typeof(IEnumerableCrmQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = scope.ServiceProvider.GetRequiredService(handlerType);

        var wrapperHandlerType = typeof(EnumerableQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var wrappedHandler = (EnumerableQueryHandler<TResult>)Activator.CreateInstance(wrapperHandlerType, handler)!;

        await foreach (var result in wrappedHandler.Execute(query, organizationService, cancellationToken))
        {
            yield return result;
        }
    }

    private abstract class QueryHandler<T>
    {
        public abstract Task<T> Execute(ICrmQuery<T> query, IOrganizationServiceAsync organizationService);
    }

    private class QueryHandler<TQuery, TResult>(ICrmQueryHandler<TQuery, TResult> innerHandler) : QueryHandler<TResult>
        where TQuery : ICrmQuery<TResult>
    {
        public override Task<TResult> Execute(
            ICrmQuery<TResult> query,
            IOrganizationServiceAsync organizationService)
        {
            return innerHandler.Execute((TQuery)query, organizationService);
        }
    }

    private abstract class EnumerableQueryHandler<T>
    {
        public abstract IAsyncEnumerable<T> Execute(IEnumerableCrmQuery<T> query, IOrganizationServiceAsync organizationService, CancellationToken cancellationToken);
    }

    private class EnumerableQueryHandler<TQuery, TResult>(IEnumerableCrmQueryHandler<TQuery, TResult> innerHandler) : EnumerableQueryHandler<TResult>
        where TQuery : IEnumerableCrmQuery<TResult>
    {
        public override IAsyncEnumerable<TResult> Execute(
            IEnumerableCrmQuery<TResult> query,
            IOrganizationServiceAsync organizationService,
            CancellationToken cancellationToken)
        {
            return innerHandler.Execute((TQuery)query, organizationService, cancellationToken);
        }
    }
}
