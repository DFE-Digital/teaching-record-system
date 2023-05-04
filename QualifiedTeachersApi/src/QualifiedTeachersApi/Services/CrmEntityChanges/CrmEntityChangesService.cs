using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Medallion.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using QualifiedTeachersApi.DataStore.Sql;

namespace QualifiedTeachersApi.Services.CrmEntityChanges;

public class CrmEntityChangesService : ICrmEntityChangesService
{
    private readonly IDbContextFactory<DqtContext> _dbContextFactory;
    private readonly IOrganizationServiceAsync _organizationService;
    private readonly IDistributedLockProvider _distributedLockProvider;

    public CrmEntityChangesService(
        IDbContextFactory<DqtContext> dbContextFactory,
        IOrganizationServiceAsync organizationService,
        IDistributedLockProvider distributedLockProvider)
    {
        _dbContextFactory = dbContextFactory;
        _organizationService = organizationService;
        _distributedLockProvider = distributedLockProvider;
    }

    public async IAsyncEnumerable<IChangedItem[]> GetEntityChanges(
        string key,
        string entityLogicalName,
        ColumnSet columns,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Ensure only one node is processing changes for this key and entity type at a time
        var @lock = await _distributedLockProvider.TryAcquireLockAsync(
            DistributedLockKeys.EntityChanges(key, entityLogicalName),
            cancellationToken: cancellationToken);

        if (@lock is null)
        {
            yield break;
        }

#pragma warning disable CS0642 // Possible mistaken empty statement
        await using (@lock) ;
#pragma warning restore CS0642 // Possible mistaken empty statement

        var entityChangesJournal = await dbContext.EntityChangesJournals
            .SingleOrDefaultAsync(t => t.Key == key && t.EntityLogicalName == entityLogicalName);

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

            var response = (RetrieveEntityChangesResponse)await _organizationService.ExecuteAsync(request);

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
                        Key = key,
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
