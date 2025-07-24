using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Polly;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillDqtIttQtsBusinessEventAuditsJob(
    [FromKeyedServices(TrsDataSyncService.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    TrsDataSyncHelper trsDataSyncHelper,
    ILogger<BackfillDqtIttQtsBusinessEventAuditsJob> logger)
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

        var columns = new ColumnSet(Contact.Fields.CreatedOn);

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

        var businessEventAuditLink = query.AddLink(
            dfeta_businesseventaudit.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            dfeta_businesseventaudit.Fields.dfeta_Person,
            JoinOperator.Inner);

        businessEventAuditLink.Columns = new ColumnSet(true);
        businessEventAuditLink.EntityAlias = dfeta_businesseventaudit.EntityLogicalName;
        businessEventAuditLink.LinkCriteria.AddCondition(
            dfeta_businesseventaudit.Fields.dfeta_changedfield,
            ConditionOperator.In,
            [
                "Early Years Teacher Status",
                "EYTS Date",
                "QTS Date",
                "Result",
                "Teacher Status"
            ]);

        var businessEventAuditCreatedByUserLink = businessEventAuditLink.AddLink(
            SystemUser.EntityLogicalName,
            dfeta_businesseventaudit.Fields.CreatedBy,
            SystemUser.PrimaryIdAttribute,
            JoinOperator.Inner);
        businessEventAuditCreatedByUserLink.Columns = new ColumnSet(
            SystemUser.PrimaryIdAttribute,
            SystemUser.Fields.FullName);
        businessEventAuditCreatedByUserLink.EntityAlias = $"{dfeta_businesseventaudit.EntityLogicalName}.{SystemUser.EntityLogicalName}_createdby";

        List<Entity> resultsForLastContact = [];

        var serviceClient = (ServiceClient)organizationService;
        DateTime crmDateFormatChangeDate = new DateTime(2010, 01, 01);

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

            // For some reason the date format in dfeta_businesseventaudit changes over the years from dd/MM/yyyy to M/d/yyyy in BUILD and PROD but not PRE-PROD!
            if (serviceClient.ConnectedOrgUniqueName == "ent-dqt-prod")
            {
                crmDateFormatChangeDate = new DateTime(2021, 01, 30);
            }
            else if (serviceClient.ConnectedOrgUniqueName == "ent-dqt-build")
            {
                crmDateFormatChangeDate = new DateTime(2019, 04, 04);
            }

            // We want to process all business event audits for a given contact at once but given we're paging we may not have every row
            // since rows could span a page boundary.
            // If there's more data available, stash the rows for the final contact and process them next time around.
            var entities = resultsForLastContact.Concat(result.Entities.ToList()).ToList();
            var lastContactId = entities.Last().Id;

            if (result.MoreRecords)
            {
                resultsForLastContact = entities.Where(e => e.Id == lastContactId).ToList();
                entities = entities.Where(e => e.Id != lastContactId).ToList();
            }

            var businessEventAudits = entities
                .Select(e => e.Extract<dfeta_businesseventaudit>())
                .Where(be => be is not null)
                .DistinctBy(be => be.Id);

            await trsDataSyncHelper.SyncQtsIttBusinessEventAuditsAsync(
                businessEventAudits,
                dryRun,
                crmDateFormatChangeDate,
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
