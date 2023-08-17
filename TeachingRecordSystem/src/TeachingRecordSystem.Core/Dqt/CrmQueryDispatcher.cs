using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.Core.Dqt;

public class CrmQueryDispatcher : ICrmQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOrganizationServiceAsync _organizationServiceAsync;

    public CrmQueryDispatcher(IServiceProvider serviceProvider, IOrganizationServiceAsync organizationServiceAsync)
    {
        _serviceProvider = serviceProvider;
        _organizationServiceAsync = organizationServiceAsync;
    }

    public async Task<TResult> ExecuteQuery<TResult>(ICrmQuery<TResult> query)
    {
        var handlerType = typeof(ICrmQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetRequiredService(handlerType);

        var wrapperHandlerType = typeof(QueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var wrappedHandler = (QueryHandler<TResult>)Activator.CreateInstance(wrapperHandlerType, handler)!;

        return await wrappedHandler.Execute(query, _organizationServiceAsync);
    }

    private abstract class QueryHandler<T>
    {
        public abstract Task<T> Execute(ICrmQuery<T> query, IOrganizationServiceAsync organizationService);
    }

    private class QueryHandler<TQuery, TResult> : QueryHandler<TResult>
        where TQuery : ICrmQuery<TResult>
    {
        private readonly ICrmQueryHandler<TQuery, TResult> _innerHandler;

        public QueryHandler(ICrmQueryHandler<TQuery, TResult> innerHandler)
        {
            _innerHandler = innerHandler;
        }

        public override Task<TResult> Execute(ICrmQuery<TResult> query, IOrganizationServiceAsync organizationService)
        {
            return _innerHandler.Execute((TQuery)query, organizationService);
        }
    }
}
