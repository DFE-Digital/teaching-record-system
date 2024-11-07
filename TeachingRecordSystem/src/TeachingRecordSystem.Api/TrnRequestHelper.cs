using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api;

public class TrnRequestHelper(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task<GetTrnRequestResult?> GetTrnRequestInfo(Guid currentApplicationUserId, string requestId)
    {
        var getDbTrnRequestTask = dbContext.TrnRequests.SingleOrDefaultAsync(r => r.ClientId == currentApplicationUserId.ToString() && r.RequestId == requestId);

        var crmTrnRequestId = GetCrmTrnRequestId(currentApplicationUserId, requestId);
        var getContactByTrnRequestIdTask = crmQueryDispatcher.ExecuteQuery(
            new GetContactByTrnRequestIdQuery(crmTrnRequestId, new Microsoft.Xrm.Sdk.Query.ColumnSet(Contact.Fields.ContactId, Contact.Fields.dfeta_TrnToken)));

        if (await getDbTrnRequestTask is TrnRequest dbTrnRequest)
        {
            return new(dbTrnRequest.TeacherId, dbTrnRequest.TrnToken);
        }

        if (await getContactByTrnRequestIdTask is Contact contact)
        {
            return new(contact.ContactId!.Value, contact.dfeta_TrnToken);
        }

        return null;
    }

    public static string GetCrmTrnRequestId(Guid currentApplicationUserId, string requestId) =>
        $"{currentApplicationUserId}::{requestId}";
}

public record GetTrnRequestResult(Guid ContactId, string? TrnToken);
