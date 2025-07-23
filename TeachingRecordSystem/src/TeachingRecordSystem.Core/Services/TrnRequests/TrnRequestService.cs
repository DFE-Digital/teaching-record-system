using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk.Query;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public class TrnRequestService(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    IGetAnIdentityApiClient idApiClient,
    IOptions<AccessYourTeachingQualificationsOptions> aytqOptionsAccessor,
    IOptions<TrnRequestOptions> trnRequestOptionsAccessor)
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
        // If we have a support task for this request, we don't need to check CRM and figure out its status here.
        // This is a little clunky currently as we can't have an FK from TrnRequestMetadata.ResolvedPersonId to Person
        // since the Person may not have synced from DQT into TRS yet.
        var fromSupportTask = await dbContext.SupportTasks
            .Include(t => t.TrnRequestMetadata)
            .Select(t => t.TrnRequestMetadata!)
            .SingleOrDefaultAsync(m => m.ApplicationUserId == applicationUserId && m.RequestId == requestId);

        if (fromSupportTask is not null)
        {
            var resolvedPersonTrn = await dbContext.Persons.Where(p => p.PersonId == fromSupportTask.ResolvedPersonId).Select(p => p.Trn).SingleAsync();
            return new GetTrnRequestResult(fromSupportTask, resolvedPersonTrn);
        }

        // If we get here then there's no support task in TRS for this request, so it must be in CRM (or not exist at all).

        var metadata = await GetRequestMetadataAsync(applicationUserId, requestId);

        if (metadata is null)
        {
            return null;
        }

        var crmTrnRequestId = GetCrmTrnRequestId(applicationUserId, requestId);
        var getContactByTrnRequestIdTask = crmQueryDispatcher.ExecuteQueryAsync(
            new GetContactByTrnRequestIdQuery(crmTrnRequestId, new ColumnSet(Contact.Fields.ContactId, Contact.Fields.dfeta_TrnToken)));

        var dbTrnRequest = await dbContext.TrnRequests.SingleOrDefaultAsync(r => r.ClientId == applicationUserId.ToString() && r.RequestId == requestId);

        Guid contactId;

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

        // If the contact returned above has a TRN then the task has been resolved in CRM and this request is completed.
        var completed = !string.IsNullOrEmpty(contact.dfeta_TRN);

        // If the request is Completed, ensure we've got a TRN and TRN token assigned to metadata as well as the resolved person's ID.
        if (completed)
        {
            if (metadata.TrnToken is null && metadata.EmailAddress is not null)
            {
                metadata.TrnToken = await CreateTrnTokenAsync(contact.dfeta_TRN, metadata.EmailAddress);
            }

            metadata.SetResolvedPerson(contact.Id);

            await dbContext.SaveChangesAsync();
        }

        return new GetTrnRequestResult(metadata, contact.dfeta_TRN);
    }

    public async Task<bool> RequiresFurtherChecksNeededSupportTaskAsync(Guid personId, Guid trnRequestApplicationUserId)
    {
        if (!trnRequestOptionsAccessor.Value.FlagFurtherChecksRequiredFromUserIds.Contains(trnRequestApplicationUserId))
        {
            return false;
        }

        var personFlags = await dbContext.Persons
            .Where(p => p.PersonId == personId)
            .Select(p => new { HasQts = p.QtsDate != null, HasEyts = p.EytsDate != null, HasOpenAlert = p.Alerts!.Any(a => a.IsOpen) })
            .SingleAsync();

        if (personFlags is { HasQts: false, HasEyts: false, HasOpenAlert: false })
        {
            return false;
        }

        return true;
    }

    public async Task CreateContactFromTrnRequestAsync(Guid applicationUserId, string requestId, Guid newContactId, string trn)
    {
        var metadata = await GetRequestMetadataAsync(applicationUserId, requestId);

        if (metadata is null)
        {
            throw new ArgumentException("TRN request does not exist.");
        }

        await CreateContactFromTrnRequestAsync(metadata, newContactId, trn);
    }

    public async Task CreateContactFromTrnRequestAsync(TrnRequestMetadata requestData, Guid newContactId, string trn)
    {
        var allowPiiUpdates = trnRequestOptionsAccessor.Value.AllowContactPiiUpdatesFromUserIds.Contains(requestData.ApplicationUserId);

        await crmQueryDispatcher.ExecuteQueryAsync(new CreateContactQuery
        {
            ContactId = newContactId,
            FirstName = requestData.FirstName!,
            MiddleName = requestData.MiddleName ?? string.Empty,
            LastName = requestData.LastName!,
            DateOfBirth = requestData.DateOfBirth,
            Gender = Contact_GenderCode.Notprovided, // TODO when we've sorted gender
            EmailAddress = requestData.EmailAddress,
            NationalInsuranceNumber = requestData.NationalInsuranceNumber,
            ApplicationUserName = requestData.ApplicationUser!.Name,
            Trn = trn,
            TrnRequestId = GetCrmTrnRequestId(requestData.ApplicationUserId,
                requestData.RequestId),
            AllowPiiUpdates = allowPiiUpdates,
            ReviewTasks = [],
            TrnRequestMetadataMessage = null
        });
    }

    public async Task UpdateContactFromTrnRequestAsync(
        Guid applicationUserId,
        string requestId,
        IReadOnlyCollection<PersonMatchedAttribute> attributesToUpdate)
    {
        var metadata = await GetRequestMetadataAsync(applicationUserId, requestId);

        if (metadata is null)
        {
            throw new ArgumentException("TRN request does not exist.");
        }

        await UpdateContactFromTrnRequestAsync(metadata, attributesToUpdate);
    }

    public Task UpdateContactFromTrnRequestAsync(
        TrnRequestMetadata requestData,
        IReadOnlyCollection<PersonMatchedAttribute> attributesToUpdate)
    {
        if (requestData.ResolvedPersonId is not Guid contactId)
        {
            throw new InvalidOperationException("TRN request is not resolved.");
        }

        var query = new UpdateContactQuery()
        {
            ContactId = contactId,
            FirstName = default,
            MiddleName = default,
            LastName = default,
            DateOfBirth = default,
            Gender = default,
            EmailAddress = default,
            NationalInsuranceNumber = default
        };

        foreach (var attribute in attributesToUpdate)
        {
            query = attribute switch
            {
                PersonMatchedAttribute.FirstName => query with { FirstName = Option.Some(requestData.FirstName!) },
                PersonMatchedAttribute.MiddleName => query with { MiddleName = Option.Some(requestData.MiddleName!) },
                PersonMatchedAttribute.LastName => query with { LastName = Option.Some(requestData.LastName!) },
                PersonMatchedAttribute.DateOfBirth => query with { DateOfBirth = Option.Some(requestData.DateOfBirth) },
                PersonMatchedAttribute.EmailAddress => query with { EmailAddress = Option.Some(requestData.EmailAddress) },
                PersonMatchedAttribute.NationalInsuranceNumber => query with { NationalInsuranceNumber = Option.Some(requestData.NationalInsuranceNumber) },
                _ => throw new NotImplementedException()
            };
        }

        return crmQueryDispatcher.ExecuteQueryAsync(query);
    }

    public static string GetCrmTrnRequestId(Guid currentApplicationUserId, string requestId) =>
        $"{currentApplicationUserId}::{requestId}";

    private Task<TrnRequestMetadata?> GetRequestMetadataAsync(Guid applicationUserId, string requestId) =>
        dbContext.TrnRequestMetadata
            .Include(m => m.ApplicationUser)
            .SingleOrDefaultAsync(r => r.ApplicationUserId == applicationUserId && r.RequestId == requestId);
}
