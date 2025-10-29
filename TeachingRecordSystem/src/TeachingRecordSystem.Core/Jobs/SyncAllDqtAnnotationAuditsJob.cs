using System.ServiceModel;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 100, DelaysInSeconds = [300])]
public class SyncAllDqtAnnotationAuditsJob(
    [FromKeyedServices(TrsDataSyncHelper.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    IDbContextFactory<TrsDbContext> dbContextFactory,
    TrsDataSyncHelper trsDataSyncHelper,
    ILogger<SyncAllDqtAnnotationAuditsJob> logger)
{
    public async Task ExecuteAsync(PerformContext context, CancellationToken cancellationToken)
    {
        var jobId = context.BackgroundJob.Id;
        var jobName = $"{nameof(SyncAllDqtAnnotationAuditsJob)}-{jobId}";

        JobMetadata? jobMetadata;
        var initialPageNumber = 1;
        string? initialPagingCookie = null;

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            jobMetadata = await dbContext.JobMetadata.SingleOrDefaultAsync(j => j.JobName == jobName, cancellationToken: cancellationToken);

            if (jobMetadata?.Metadata.GetValueOrDefault(MetadataKeys.PageNumber) is string pageNumberMetadata)
            {
                initialPageNumber = int.Parse(pageNumberMetadata);
            }
            if (jobMetadata?.Metadata.GetValueOrDefault(MetadataKeys.PagingCoookie) is string pagingCookieMetadata)
            {
                initialPagingCookie = pagingCookieMetadata;
            }

            if (jobMetadata is null)
            {
                jobMetadata = new JobMetadata { JobName = jobName, Metadata = new() };
                dbContext.JobMetadata.Add(jobMetadata);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        const int pageSize = 1000;

        var query = new QueryExpression(Annotation.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(),
            Orders =
            {
                new OrderExpression(Annotation.Fields.CreatedOn, OrderType.Ascending),
                new OrderExpression(Annotation.PrimaryIdAttribute, OrderType.Ascending)
            },
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = initialPageNumber,
                PagingCookie = initialPagingCookie
            }
        };

        var fetched = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            EntityCollection result;
            try
            {
                result = await organizationService.RetrieveMultipleAsync(query);
            }
            catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
            {
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            fetched += result.Entities.Count;

            await trsDataSyncHelper.SyncAuditAsync(
                Annotation.EntityLogicalName,
                result.Entities.Select(e => e.Id),
                skipIfExists: false,
                cancellationToken);

            if (fetched > 0 && fetched % 50000 == 0)
            {
                logger.LogWarning("Synced {Count} annotation audit records.", fetched);
            }

            if (result.MoreRecords)
            {
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = result.PagingCookie;

                await using (var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
                {
                    dbContext.Attach(jobMetadata);

                    jobMetadata.Metadata[MetadataKeys.PageNumber] = query.PageInfo.PageNumber.ToString();
                    jobMetadata.Metadata[MetadataKeys.PagingCoookie] = result.PagingCookie;

                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                await using (var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
                {
                    await dbContext.JobMetadata.Where(j => j.JobName == jobName).ExecuteDeleteAsync();
                }

                break;
            }
        }

        logger.LogWarning("Synced {Count} annotation audit records.", fetched);
    }

    private static class MetadataKeys
    {
        public const string PageNumber = nameof(PageNumber);
        public const string PagingCoookie = nameof(PagingCoookie);
    }
}
