using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api;

public class TrnRequestHelper(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher, IClock clock)
{
    public async Task<GetTrnRequestResult?> GetTrnRequestInfo(Guid currentApplicationUserId, string requestId)
    {
        var getDbTrnRequestTask = dbContext.TrnRequests.SingleOrDefaultAsync(r => r.ClientId == currentApplicationUserId.ToString() && r.RequestId == requestId);

        var crmTrnRequestId = GetCrmTrnRequestId(currentApplicationUserId, requestId);
        var getContactByTrnRequestIdTask = crmQueryDispatcher.ExecuteQuery(
            new GetContactByTrnRequestIdQuery(crmTrnRequestId, new Microsoft.Xrm.Sdk.Query.ColumnSet(Contact.Fields.ContactId, Contact.Fields.dfeta_TrnToken)));

        // We can't have this running in parallel with getDbTrnRequestTask since they share a connection so make it continuation
        var metadata = await getDbTrnRequestTask
            .ContinueWith(_ => dbContext.TrnRequestMetadata
                .SingleOrDefaultAsync(m => m.ApplicationUserId == currentApplicationUserId && m.RequestId == requestId))
            .Unwrap();

        if (await getDbTrnRequestTask is TrnRequest dbTrnRequest)
        {
            return new(dbTrnRequest.TeacherId, dbTrnRequest.TrnToken, metadata, currentApplicationUserId);
        }

        if (await getContactByTrnRequestIdTask is Contact contact)
        {
            return new(contact.ContactId!.Value, contact.dfeta_TrnToken, metadata, currentApplicationUserId);
        }

        return null;
    }

    public static string GetCrmTrnRequestId(Guid currentApplicationUserId, string requestId) =>
        $"{currentApplicationUserId}::{requestId}";

    public async Task EnsureOneLoginUserIsConnected(GetTrnRequestResult trnRequest, Contact contact)
    {
        if (trnRequest.Metadata?.VerifiedOneLoginUserSubject is not string oneLoginUserSubject)
        {
            return;
        }

        if (await dbContext.OneLoginUsers.AnyAsync(u => u.Subject == oneLoginUserSubject))
        {
            return;
        }

        Debug.Assert(contact.dfeta_TRN is not null);

        var oneLoginUser = new OneLoginUser() { Subject = oneLoginUserSubject };

        var verifiedName = new string[]
        {
            contact.HasStatedNames() ? contact.dfeta_StatedFirstName : contact.FirstName,
            contact.HasStatedNames() ? contact.dfeta_StatedMiddleName : contact.MiddleName,
            contact.HasStatedNames() ? contact.dfeta_StatedLastName : contact.LastName
        };

        var verifiedDateOfBirth = contact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false);

        oneLoginUser.SetVerified(
            verifiedOn: clock.UtcNow,
            OneLoginUserVerificationRoute.External,
            verifiedByApplicationUserId: trnRequest.ApplicationUserId,
            verifiedNames: [[.. verifiedName]],
            verifiedDatesOfBirth: [verifiedDateOfBirth]);

        oneLoginUser.SetMatched(contact.Id, OneLoginUserMatchRoute.TrnAllocation, matchedAttributes: null);

        dbContext.OneLoginUsers.Add(oneLoginUser);

        await dbContext.SaveChangesAsync();
    }
}

public record GetTrnRequestResult(Guid ContactId, string? TrnToken, TrnRequestMetadata? Metadata, Guid ApplicationUserId);
