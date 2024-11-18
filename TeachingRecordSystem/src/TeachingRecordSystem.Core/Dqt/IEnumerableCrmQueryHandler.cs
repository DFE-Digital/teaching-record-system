using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.Core.Dqt;

public interface IEnumerableCrmQueryHandler<TQuery, TResult>
    where TQuery : IEnumerableCrmQuery<TResult>
{
    IAsyncEnumerable<TResult> ExecuteAsync(TQuery query, IOrganizationServiceAsync organizationService, CancellationToken cancellationToken);
}
