using System.Diagnostics;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core;

public class TrnRequestHelper(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher, IClock clock)
{
    public async Task<GetTrnRequestResult?> GetTrnRequestInfo(Guid applicationUserId, string requestId)
    {
        var getDbTrnRequestTask = dbContext.TrnRequests.SingleOrDefaultAsync(r => r.ClientId == applicationUserId.ToString() && r.RequestId == requestId);

        var crmTrnRequestId = GetCrmTrnRequestId(applicationUserId, requestId);
        var getContactByTrnRequestIdTask = crmQueryDispatcher.ExecuteQuery(
            new GetContactByTrnRequestIdQuery(crmTrnRequestId, new ColumnSet(Contact.Fields.ContactId, Contact.Fields.dfeta_TrnToken)));

        if (await getDbTrnRequestTask is TrnRequest dbTrnRequest)
        {
            var contact = await GetContact(dbTrnRequest.TeacherId);
            return new(dbTrnRequest.TrnToken, applicationUserId, contact, IsCompleted(contact));
        }

        if (await getContactByTrnRequestIdTask is Contact trnRequestContact)
        {
            var contact = await GetContact(trnRequestContact.Id);
            return new(contact.dfeta_TrnToken, applicationUserId, contact, IsCompleted(contact));
        }

        return null;

        bool IsCompleted(Contact contact) => !string.IsNullOrEmpty(contact.dfeta_TRN);

        Task<Contact> GetContact(Guid contactId) =>
            crmQueryDispatcher.ExecuteQuery(
                new GetContactWithMergeResolutionQuery(
                    contactId,
                    new ColumnSet(
                        Contact.Fields.dfeta_TRN,
                        Contact.Fields.FirstName,
                        Contact.Fields.MiddleName,
                        Contact.Fields.LastName,
                        Contact.Fields.dfeta_StatedFirstName,
                        Contact.Fields.dfeta_StatedMiddleName,
                        Contact.Fields.dfeta_StatedLastName,
                        Contact.Fields.EMailAddress1,
                        Contact.Fields.dfeta_NINumber,
                        Contact.Fields.BirthDate,
                        Contact.Fields.Merged,
                        Contact.Fields.MasterId,
                        Contact.Fields.dfeta_SlugId,
                        Contact.Fields.dfeta_QTSDate,
                        Contact.Fields.dfeta_TrnToken)));
    }

    public static string GetCrmTrnRequestId(Guid currentApplicationUserId, string requestId) =>
        $"{currentApplicationUserId}::{requestId}";

    public Task<TrnRequestMetadata?> GetRequestMetadata(Guid applicationUserId, string requestId) =>
        dbContext.TrnRequestMetadata.SingleOrDefaultAsync(m => m.ApplicationUserId == applicationUserId && m.RequestId == requestId);

    public async Task EnsureOneLoginUserIsConnected(GetTrnRequestResult trnRequest, string oneLoginUserSubject)
    {
        if (await dbContext.OneLoginUsers.AnyAsync(u => u.Subject == oneLoginUserSubject))
        {
            return;
        }

        var contact = trnRequest.Contact;

        Debug.Assert(contact.dfeta_TRN is not null);

        var oneLoginUser = new OneLoginUser() { Subject = oneLoginUserSubject };

        var verifiedName = new List<string>()
        {
            contact.HasStatedNames() ? contact.dfeta_StatedFirstName : contact.FirstName,
            contact.HasStatedNames() ? contact.dfeta_StatedMiddleName : contact.MiddleName,
            contact.HasStatedNames() ? contact.dfeta_StatedLastName : contact.LastName
        };

        if (string.IsNullOrEmpty(verifiedName[1]))
        {
            verifiedName.RemoveAt(1);
        }

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

public record GetTrnRequestResult(string? TrnToken, Guid ApplicationUserId, Contact Contact, bool Completed);
