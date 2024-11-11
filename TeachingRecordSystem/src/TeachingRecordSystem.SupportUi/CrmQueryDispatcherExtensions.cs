using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.SupportUi;

public static class CrmQueryDispatcherExtensions
{
    public static ICrmQueryDispatcher WithDqtUserImpersonation(this ICrmQueryDispatcher dispatcher) =>
        new CrmQueryDispatcherWithDqtUserImpersonation(dispatcher as CrmQueryDispatcher ??
            throw new InvalidOperationException($"{nameof(ICrmQueryDispatcher)} is not a {nameof(CrmQueryDispatcher)}."));

    private class CrmQueryDispatcherWithDqtUserImpersonation(CrmQueryDispatcher innerDispatcher) : ICrmQueryDispatcher
    {
        public Task<TResult> ExecuteQuery<TResult>(ICrmQuery<TResult> query) =>
            innerDispatcher.ExecuteQuery(GetOrganizationService, query);

        public IAsyncEnumerable<TResult> ExecuteQuery<TResult>(IEnumerableCrmQuery<TResult> query, CancellationToken cancellationToken = default) =>
            innerDispatcher.ExecuteQuery(GetOrganizationService, query, cancellationToken);

        public CrmTransactionScope CreateTransactionRequestBuilder() =>
            throw new NotSupportedException();

        private IOrganizationServiceAsync GetOrganizationService(IServiceProvider serviceProvider)
        {
            var organizationService = innerDispatcher.GetOrganizationService(serviceProvider);

            // Horrible check if to see if we're running Tests with FakeXrmEasy. If we are, we can't do impersonation.
            if (organizationService.GetType().FullName == "Castle.Proxies.ObjectProxy_2")
            {
                return organizationService;
            }

            var serviceClient = organizationService as ServiceClient ??
                throw new InvalidOperationException($"{nameof(IOrganizationServiceAsync)} is not a {nameof(ServiceClient)}.");

            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext.");
            var dqtUserId = httpContext.User.GetDqtUserId();

            serviceClient.CallerId = dqtUserId;

            return serviceClient;
        }
    }
}
