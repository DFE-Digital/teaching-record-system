using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.Core.Dqt;

public interface ICrmQueryHandler<TQuery, TResult>
    where TQuery : ICrmQuery<TResult>
{
    Task<TResult> ExecuteAsync(TQuery query, IOrganizationServiceAsync organizationService);
}
