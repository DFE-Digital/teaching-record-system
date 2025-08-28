using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Polly;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class MigrateRoutesFromCrmJob(
    [FromKeyedServices(TrsDataSyncHelper.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    TrsDataSyncHelper trsDataSyncHelper,
    IOptions<TrsDataSyncServiceOptions> syncOptionsAccessor,
    ILogger<MigrateRoutesFromCrmJob> logger)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        var resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions()
            {
                BackoffType = DelayBackoffType.Linear,
                Delay = TimeSpan.FromSeconds(30),
                MaxRetryAttempts = 10
            })
            .Build();

        const int pageSize = 1000;

        var columns = new ColumnSet(
            Contact.Fields.dfeta_qtlsdate,
            Contact.Fields.dfeta_QtlsDateHasBeenSet,
            Contact.Fields.CreatedOn);

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = columns,
            Orders =
        {
            new OrderExpression(Contact.Fields.CreatedOn, OrderType.Ascending),
            new OrderExpression(Contact.PrimaryIdAttribute, OrderType.Ascending)
        },
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = 1
            }
        };

        var ittLink = query.AddLink(
            dfeta_initialteachertraining.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            dfeta_initialteachertraining.Fields.dfeta_PersonId,
            JoinOperator.LeftOuter);
        ittLink.Columns = new ColumnSet(true);
        ittLink.EntityAlias = dfeta_initialteachertraining.EntityLogicalName;

        var qtsLink = query.AddLink(
            dfeta_qtsregistration.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            dfeta_qtsregistration.Fields.dfeta_PersonId,
            JoinOperator.LeftOuter);
        qtsLink.Columns = new ColumnSet(true);
        qtsLink.EntityAlias = dfeta_qtsregistration.EntityLogicalName;

        List<Entity> resultsForLastContact = [];

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            async Task<EntityCollection> QueryAsync()
            {
                try
                {
                    return await organizationService.RetrieveMultipleAsync(query);
                }
                catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
                {
                    logger.LogWarning("Hit CRM service limits; error code: {ErrorCode}.  Retrying after {retryAfter} seconds.", fex.Detail.ErrorCode, retryAfter.TotalSeconds);
                    await Task.Delay(retryAfter);
                    return await QueryAsync();
                }
            }

            var result = await resiliencePipeline.ExecuteAsync(async _ => await QueryAsync());

            // We need to process all QTS and ITT for a given contact at once but given we're paging we may not have every row
            // since rows could span a page boundary.
            // If there's more data available, stash the rows for the final contact and process them next time around.
            var entities = resultsForLastContact.Concat(result.Entities.ToList()).ToList();
            var lastContactId = entities.Last().Id;

            if (result.MoreRecords)
            {
                resultsForLastContact = entities.Where(e => e.Id == lastContactId).ToList();
                entities = entities.Where(e => e.Id != lastContactId).ToList();
            }

            var contacts = entities
                .Select(e => e.ToEntity<Contact>())
                .DistinctBy(c => c.Id);

            var qts = entities
                .Select(e => e.Extract<dfeta_qtsregistration>())
                .Where(qts => qts is not null)
                .DistinctBy(qts => qts.Id);

            var itt = entities
                .Select(e => e.Extract<dfeta_initialteachertraining>())
                .Where(itt => itt is not null)
                .DistinctBy(itt => itt.Id);

            await trsDataSyncHelper.MigrateIttAndQtsRegistrationsAsync(
                contacts.ToArray(),
                qts.ToArray(),
                itt.ToArray(),
                ignoreInvalid: syncOptionsAccessor.Value.IgnoreInvalidData,
                dryRun,
                cancellationToken);

            if (result.MoreRecords)
            {
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = result.PagingCookie;
            }
            else
            {
                break;
            }
        }
    }
}
