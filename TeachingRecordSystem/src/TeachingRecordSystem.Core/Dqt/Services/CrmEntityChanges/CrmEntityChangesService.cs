using System.Runtime.CompilerServices;
using System.ServiceModel;
using Medallion.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Dqt.Services.CrmEntityChanges;

public class CrmEntityChangesService : ICrmEntityChangesService
{
    private readonly IDbContextFactory<TrsDbContext> _dbContextFactory;
    private readonly ICrmServiceClientProvider _crmServiceClientProvider;
    private readonly IDistributedLockProvider _distributedLockProvider;

    public CrmEntityChangesService(
        IDbContextFactory<TrsDbContext> dbContextFactory,
        ICrmServiceClientProvider crmServiceClientProvider,
        IDistributedLockProvider distributedLockProvider)
    {
        _dbContextFactory = dbContextFactory;
        _crmServiceClientProvider = crmServiceClientProvider;
        _distributedLockProvider = distributedLockProvider;
    }

    public async IAsyncEnumerable<IChangedItem[]> GetEntityChanges(
        string changesKey,
        string crmClientName,
        string entityLogicalName,
        ColumnSet columns,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // CRM ignores page sizes above 5000
        if (pageSize > 5000)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        // Ensure only one node is processing changes for this key and entity type at a time
        var @lock = await _distributedLockProvider.TryAcquireLockAsync(
            DistributedLockKeys.EntityChanges(changesKey, entityLogicalName),
            cancellationToken: cancellationToken);

        if (@lock is null)
        {
            yield break;
        }

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        await using (@lock)
        {
            var entityChangesJournal = await dbContext.EntityChangesJournals
                .SingleOrDefaultAsync(t => t.Key == changesKey && t.EntityLogicalName == entityLogicalName);

            var organizationService = _crmServiceClientProvider.GetClient(changesKey);

            var request = new RetrieveEntityChangesRequest()
            {
                Columns = columns,
                EntityName = entityLogicalName,
                PageInfo = new()
                {
                    Count = pageSize,
                    PageNumber = 1
                },
                DataVersion = entityChangesJournal?.DataToken
            };

            var gotData = false;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                RetrieveEntityChangesResponse response;
                try
                {
                    response = (RetrieveEntityChangesResponse)await organizationService.ExecuteAsync(request);
                }
                catch (FaultException<OrganizationServiceFault> fault) when (fault.Detail.ErrorCode == -2147204270)  // ExpiredVersionStamp
                {
                    // If entity metadata has changed we get an error:
                    // Version stamp associated with the client has expired. Please perform a full sync.
                    // Resetting DataVersion will give us a full sync.
                    request.DataVersion = null;
                    continue;
                }
                catch (InsufficientMemoryException) when (request.PageInfo.Count > 1)
                {
                    request.PageInfo.Count /= 2;
                    request.PageInfo.PageNumber = 1;
                    request.PageInfo.PagingCookie = null;
                    continue;
                }

                gotData |= response.EntityChanges.Changes.Count > 0;

                if (response.EntityChanges.Changes.Count > 0)
                {
                    yield return response.EntityChanges.Changes.ToArray();
                }

                if (!response.EntityChanges.MoreRecords)
                {
                    if (gotData && entityChangesJournal is not null)
                    {
                        entityChangesJournal.DataToken = response.EntityChanges.DataToken;
                        await dbContext.SaveChangesAsync();
                    }
                    else if (entityChangesJournal is null)
                    {
                        dbContext.EntityChangesJournals.Add(new()
                        {
                            Key = changesKey,
                            EntityLogicalName = entityLogicalName,
                            DataToken = response.EntityChanges.DataToken
                        });
                        await dbContext.SaveChangesAsync();
                    }

                    break;
                }

                request.PageInfo.PageNumber++;
                request.PageInfo.PagingCookie = response.EntityChanges.PagingCookie;
            }
        }
    }
}
