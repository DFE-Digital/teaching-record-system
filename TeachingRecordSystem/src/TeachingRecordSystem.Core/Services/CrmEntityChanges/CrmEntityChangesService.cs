using System.Runtime.CompilerServices;
using System.ServiceModel;
using Medallion.Threading;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Services.CrmEntityChanges;

public class CrmEntityChangesService(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IOrganizationServiceAsync organizationService,
    IDistributedLockProvider distributedLockProvider,
    IClock clock) : ICrmEntityChangesService
{
    public async IAsyncEnumerable<IChangedItem[]> GetEntityChanges(
        string changesKey,
        string entityLogicalName,
        ColumnSet columns,
        DateTime? modifiedSince,
        int pageSize,
        bool rollUpChanges,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // CRM ignores page sizes above 5000
        if (pageSize > 5000)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        // Ensure only one node is processing changes for this key and entity type at a time
        var @lock = await distributedLockProvider.TryAcquireLockAsync(
            DistributedLockKeys.EntityChanges(changesKey, entityLogicalName),
            cancellationToken: cancellationToken);

        if (@lock is null)
        {
            yield break;
        }

        // If we're filtering out records that came before modifiedSince, ensure we have the modifiedon attribute
        var columnSet =
            modifiedSince.HasValue && !columns.Columns.Contains("modifiedon") && !columns.AllColumns ?
            new ColumnSet(columns.Columns.Append("modifiedon").ToArray()) :
            columns;

        using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await using (@lock)
        {
            var request = new RetrieveEntityChangesRequest()
            {
                Columns = columnSet,
                EntityName = entityLogicalName,
                PageInfo = new()
                {
                    Count = pageSize,
                    PageNumber = 1
                }
            };

            var entityChangesJournal = await dbContext.EntityChangesJournals
                .SingleOrDefaultAsync(t => t.Key == changesKey && t.EntityLogicalName == entityLogicalName);

            if (entityChangesJournal is not null)
            {
                request.DataVersion = entityChangesJournal.DataToken;

                if (entityChangesJournal.NextQueryPageSize == pageSize)
                {
                    request.PageInfo.PageNumber = entityChangesJournal.NextQueryPageNumber ?? 1;
                    request.PageInfo.PagingCookie = entityChangesJournal.NextQueryPagingCookie;
                }
            }

            var queryCount = 0;

            while (true)
            {
                // Persist the paging information to the DB so that if we restart we can pick up where we left off.
                // This is particularly important for when we have to a do a full sync as we're often deploying so regularly we can't quite catch up.
                if (queryCount > 0)
                {
                    await UpsertEntityChangesJournal(
                        request.DataVersion,
                        request.PageInfo.PageNumber,
                        request.PageInfo.Count,
                        request.PageInfo.PagingCookie);
                }

                cancellationToken.ThrowIfCancellationRequested();

                RetrieveEntityChangesResponse response;
                try
                {
                    queryCount++;
                    response = (RetrieveEntityChangesResponse)await organizationService.ExecuteAsync(request);
                }
                catch (FaultException<OrganizationServiceFault> fault) when (fault.Detail.ErrorCode == -2147204270)  // ExpiredVersionStamp
                {
                    // If entity metadata has changed we get an error:
                    // Version stamp associated with the client has expired. Please perform a full sync.
                    // Resetting DataVersion will give us a full sync.
                    request.DataVersion = null;
                    request.PageInfo.PageNumber = 1;
                    request.PageInfo.PagingCookie = null;
                    continue;
                }
                catch (Exception ex) when ((ex is InsufficientMemoryException || ex is OutOfMemoryException) && request.PageInfo.Count > 1)
                {
                    // REVIEW: We could be a little smarter here; we need to query with a smaller page size to prevent us running out of memory.
                    // We need to make sure we don't miss anything but we don't want to be re-processing data we've already processed either.
                    // If we're halving the page size then doubling the page number could work but I don't know whether that will work with
                    // a PagingCookie that used a different page size.

                    request.PageInfo.Count /= 2;
                    request.PageInfo.PageNumber = 1;
                    request.PageInfo.PagingCookie = null;
                    continue;
                }
                catch (Exception ex) when (ex.IsCrmRateLimitException(out var retryAfter))
                {
                    await Task.Delay(retryAfter, cancellationToken);
                    continue;
                }

                // Filter out any changes that came before modifiedSince (if it's non-null).
                // Note that this is a greater than *or equal to* operator since it's possible there are multiple changes at exactly the same time
                // and we want to ensure we don't miss any of them.
                var changes = response.EntityChanges.Changes
                    .Where(e => !modifiedSince.HasValue || e is not NewOrUpdatedItem ||
                        e is NewOrUpdatedItem newOrUpdatedItem && newOrUpdatedItem.NewOrUpdatedEntity.GetAttributeValue<DateTime>("modifiedon") >= modifiedSince.Value)
                    .ToArray();

                // Roll up changes to the same record so callers don't get the same record more than once in a batch.
                if (rollUpChanges)
                {
                    changes = changes
                        .GroupBy(e =>
                            e is NewOrUpdatedItem newOrUpdatedItem ? newOrUpdatedItem.NewOrUpdatedEntity.Id :
                            e is RemovedOrDeletedItem removedOrDeletedItem ? removedOrDeletedItem.RemovedItem.Id :
                            throw new NotSupportedException($"Unexpected ChangeType: '{e.Type}'."))
                        .Select(g => g.Last())
                        .ToArray();
                }

                if (changes.Length > 0)
                {
                    yield return changes;
                }

                if (!response.EntityChanges.MoreRecords)
                {
                    await UpsertEntityChangesJournal(
                        response.EntityChanges.DataToken,
                        nextQueryPageNumber: null,
                        nextQueryPageSize: null,
                        nextQueryPagingCookie: null);

                    break;
                }

                request.PageInfo.PageNumber++;
                request.PageInfo.PagingCookie = response.EntityChanges.PagingCookie;
            }
        }

        Task UpsertEntityChangesJournal(
            string? dataToken,
            int? nextQueryPageNumber,
            int? nextQueryPageSize,
            string? nextQueryPagingCookie)
        {
            var hostName = System.Net.Dns.GetHostName();

            return dbContext.Database.ExecuteSqlAsync(
                $"""
                INSERT INTO entity_changes_journals
                (key, entity_logical_name, data_token, last_updated, last_updated_by, next_query_page_number, next_query_page_size, next_query_paging_cookie)
                VALUES ({changesKey}, {entityLogicalName}, {dataToken}, {clock.UtcNow}, {hostName}, {nextQueryPageNumber}, {nextQueryPageSize}, {nextQueryPagingCookie})
                ON CONFLICT (key, entity_logical_name) DO UPDATE SET
                data_token = EXCLUDED.data_token,
                last_updated = EXCLUDED.last_updated,
                last_updated_by = EXCLUDED.last_updated_by,
                next_query_page_number = EXCLUDED.next_query_page_number,
                next_query_page_size = EXCLUDED.next_query_page_size,
                next_query_paging_cookie = EXCLUDED.next_query_paging_cookie
                """);
        }
    }
}
