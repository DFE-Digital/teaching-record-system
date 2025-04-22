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

        Guid contactId;

        if (metadata is null)
        {
            return null;
        }

        if (metadata.ResolvedPersonId is Guid personId)
        {
            contactId = personId;
        }
        else if (dbTrnRequest is not null)
        {
            contactId = dbTrnRequest.TeacherId;
        }
        else if (await getContactByTrnRequestIdTask is Contact trnRequestContact)
        {
            contactId = trnRequestContact.Id;
        }
        else
        {
            return null;
        }

        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
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

        var result = new GetTrnRequestResult(applicationUserId, contact, metadata);

        // If the request is Completed, ensure we've got a TRN and TRN token assigned to metadata
        if (result.IsCompleted)
        {
            if (metadata.TrnToken is null && metadata.EmailAddress is not null)
            {
                metadata.TrnToken = await CreateTrnTokenAsync(result.Trn, metadata.EmailAddress);
            }

            metadata.ResolvedPersonId ??= result.Contact.Id;

            await dbContext.SaveChangesAsync();
        }

        return result;
    }

    public static string GetCrmTrnRequestId(Guid currentApplicationUserId, string requestId) =>
        $"{currentApplicationUserId}::{requestId}";
}

public record GetTrnRequestResult(Guid ApplicationUserId, Contact Contact, TrnRequestMetadata Metadata)
{
    public string? Trn => Contact.dfeta_TRN;
    public bool PotentialDuplicate => Metadata.PotentialDuplicate == true;
    public string? TrnToken => Metadata.TrnToken;
    [MemberNotNullWhen(true, nameof(Trn))]
    public bool IsCompleted => Trn is not null;
}
