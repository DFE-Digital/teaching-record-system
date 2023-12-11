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
