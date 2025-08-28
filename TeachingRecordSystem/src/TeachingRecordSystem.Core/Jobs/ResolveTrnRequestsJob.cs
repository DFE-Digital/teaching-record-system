using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core.Jobs;

public class ResolveTrnRequestsJob(IDbContextFactory<TrsDbContext> dbContextFactory, ICrmQueryDispatcher crmQueryDispatcher, IOrganizationServiceAsync2 serviceClient)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var readDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        readDbContext.Database.SetCommandTimeout(0);

        var unresolvedRequests = readDbContext.TrnRequestMetadata
            .Where(m => m.ResolvedPersonId == null)
            .Select(m => new { m.ApplicationUserId, m.RequestId })
            .ToAsyncEnumerable();

        await foreach (var requestChunk in unresolvedRequests.ChunkAsync(50).WithCancellation(cancellationToken))
        {
            var queryByAttribute = new QueryExpression()
            {
                EntityName = Contact.EntityLogicalName,
                ColumnSet = new ColumnSet(
                    Contact.Fields.dfeta_TrnRequestID,
                    Contact.Fields.StateCode,
                    Contact.Fields.MasterId)
            };
            queryByAttribute.Criteria.AddCondition(
                Contact.Fields.dfeta_TrnRequestID,
                ConditionOperator.In,
                requestChunk.Select(m => TrnRequestService.GetCrmTrnRequestId(m.ApplicationUserId, m.RequestId)).Cast<object>().ToArray());

            var response = await serviceClient.RetrieveMultipleAsync(queryByAttribute);

            var contactByRequestId = response.Entities.Select(e => e.ToEntity<Contact>())
                .ToDictionary(e => e.dfeta_TrnRequestID, e => e);

            await using var writeDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            foreach (var request in requestChunk)
            {
                var crmContact = contactByRequestId.GetValueOrDefault(
                    TrnRequestService.GetCrmTrnRequestId(request.ApplicationUserId, request.RequestId));

                Guid initialContactId;
                if (crmContact is null)
                {
                    var dbTrnRequest = await writeDbContext.TrnRequests
                        .SingleOrDefaultAsync(r => r.ClientId == request.ApplicationUserId.ToString() && r.RequestId == request.RequestId);

                    if (dbTrnRequest is null)
                    {
                        continue;
                    }

                    initialContactId = dbTrnRequest.TeacherId;
                }
                else
                {
                    initialContactId = crmContact.Id;
                }

                var resolvedContact = await crmQueryDispatcher.ExecuteQueryAsync(
                    new GetContactWithMergeResolutionQuery(initialContactId, new ColumnSet(Contact.Fields.dfeta_TRN)));

                var metadata = (await writeDbContext.TrnRequestMetadata.FindAsync([request.ApplicationUserId, request.RequestId], cancellationToken))!;

                if (resolvedContact?.dfeta_TRN is null)
                {
                    if (metadata.ResolvedPersonId.HasValue)
                    {
                        metadata.ResolvedPersonId = null;
                        metadata.Status = TrnRequestStatus.Pending;
                    }

                    continue;
                }

                metadata.SetResolvedPerson(resolvedContact.Id);
            }

            await writeDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
