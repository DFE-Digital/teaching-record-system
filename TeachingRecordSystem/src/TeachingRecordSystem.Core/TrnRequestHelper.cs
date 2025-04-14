using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Core;

public class TrnRequestHelper(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    IGetAnIdentityApiClient idApiClient,
    IOptions<AccessYourTeachingQualificationsOptions> aytqOptionsAccessor)
{
    public async Task<string> CreateTrnTokenAsync(string trn, string emailAddress)
    {
        var response = await idApiClient.CreateTrnTokenAsync(new CreateTrnTokenRequest() { Email = emailAddress, Trn = trn });
        return response.TrnToken;
    }

    public string GetAccessYourTeachingQualificationsLink(string trnToken) =>
        $"{aytqOptionsAccessor.Value.BaseAddress}{aytqOptionsAccessor.Value.StartUrlPath}?trn_token={Uri.EscapeDataString(trnToken)}";

    public async Task<GetTrnRequestResult?> GetTrnRequestInfoAsync(Guid applicationUserId, string requestId)
    {
        var crmTrnRequestId = GetCrmTrnRequestId(applicationUserId, requestId);
        var getContactByTrnRequestIdTask = crmQueryDispatcher.ExecuteQueryAsync(
            new GetContactByTrnRequestIdQuery(crmTrnRequestId, new ColumnSet(Contact.Fields.ContactId, Contact.Fields.dfeta_TrnToken)));

        var dbTrnRequest = await dbContext.TrnRequests.SingleOrDefaultAsync(r => r.ClientId == applicationUserId.ToString() && r.RequestId == requestId);
        var metadata = await dbContext.TrnRequestMetadata.SingleOrDefaultAsync(r => r.ApplicationUserId == applicationUserId && r.RequestId == requestId);

        if (metadata is null)
        {
            return null;
        }

        if (dbTrnRequest is not null)
        {
            var contact = await GetContactAsync(dbTrnRequest.TeacherId);
            return new(applicationUserId, contact, metadata);
        }

        if (await getContactByTrnRequestIdTask is Contact trnRequestContact)
        {
            var contact = await GetContactAsync(trnRequestContact.Id);
            return new(applicationUserId, contact, metadata);
        }

        return null;

        Task<Contact> GetContactAsync(Guid contactId) =>
            crmQueryDispatcher.ExecuteQueryAsync(
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

    public Task<TrnRequestMetadata?> GetRequestMetadataAsync(Guid applicationUserId, string requestId) =>
        dbContext.TrnRequestMetadata.SingleOrDefaultAsync(m => m.ApplicationUserId == applicationUserId && m.RequestId == requestId);
}

public record GetTrnRequestResult(Guid ApplicationUserId, Contact Contact, TrnRequestMetadata Metadata)
{
    public string? Trn => Contact.dfeta_TRN;
    public bool PotentialDuplicate => Metadata.PotentialDuplicate == true;
    public string? TrnToken => Metadata.TrnToken;
    [MemberNotNullWhen(true, nameof(Trn))]
    public bool IsCompleted => Trn is not null;
}
