using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core;

public class TrnRequestHelper(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher)
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
}

public record GetTrnRequestResult(string? TrnToken, Guid ApplicationUserId, Contact Contact, bool IsCompleted);
