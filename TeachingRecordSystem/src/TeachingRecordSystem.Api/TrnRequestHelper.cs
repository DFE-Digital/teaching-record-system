using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api;

public class TrnRequestHelper(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task<GetTrnRequestResult?> GetTrnRequestInfo(string currentClientId, string requestId)
    {
        var getDbTrnRequestTask = dbContext.TrnRequests.SingleOrDefaultAsync(r => r.ClientId == currentClientId && r.RequestId == requestId);

        var crmTrnRequestId = GetCrmTrnRequestId(currentClientId, requestId);
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

    public static string GetCrmTrnRequestId(string currentClientId, string requestId) =>
        $"{currentClientId}::{requestId}";
}

public record GetTrnRequestResult(Guid ContactId, string? TrnToken);
