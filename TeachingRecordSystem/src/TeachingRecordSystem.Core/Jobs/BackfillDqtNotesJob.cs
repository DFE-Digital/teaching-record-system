using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillDqtNotesJob([FromKeyedServices(TrsDataSyncService.CrmClientName)]
    IOrganizationServiceAsync2 organizationService,
    TrsDataSyncHelper trsDataSyncHelper,
    ILogger<BackfillDqtNotesJob> logger,
    TrsDbContext dbContext)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        const int pageSize = 100;

        var defaultMinCreatedOn = new DateTime(2014, 01, 01);
        var minCreatedOn = await dbContext.Notes.MaxAsync(n => (DateTime?)n.CreatedOn);

        var query = new QueryExpression(Annotation.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Annotation.Fields.CreatedBy,
                Annotation.Fields.CreatedOn,
                Annotation.Fields.ModifiedBy,
                Annotation.Fields.ModifiedOn,
                Annotation.Fields.NoteText,
                Annotation.Fields.FileName,
                Annotation.Fields.MimeType,
                Annotation.Fields.DocumentBody,
                Annotation.Fields.ObjectId,
                Annotation.Fields.Subject
            ),
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = 1
            },
            Orders =
            {
                new OrderExpression
                {
                    AttributeName = Annotation.Fields.CreatedOn,
                    OrderType = OrderType.Ascending
                }
            }
        };
        query.Criteria.AddCondition(Annotation.Fields.ObjectId, ConditionOperator.NotNull);
        query.Criteria.AddCondition(Annotation.Fields.ObjectTypeCode, ConditionOperator.Equal, Contact.EntityLogicalName);
        query.Criteria.AddCondition(Annotation.Fields.CreatedOn, ConditionOperator.GreaterThan, minCreatedOn ?? defaultMinCreatedOn);

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

            await trsDataSyncHelper.SyncAnnotationsAsync(
                result.Entities.Select(e => e.ToEntity<Annotation>()).ToArray(),
                true,
                false,
                cancellationToken);

            if (fetched > 0 && fetched % 50000 == 0)
            {
                logger.LogWarning("Synced {Count} Annotation audit records.", fetched);
            }

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
