using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetTrnRequestCommand(string RequestId);

public class GetTrnRequestHandler(
    ICrmQueryDispatcher crmQueryDispatcher,
    TrnRequestHelper trnRequestHelper,
    ICurrentUserProvider currentUserProvider)
{
    public async Task<TrnRequestInfo?> Handle(GetTrnRequestCommand command)
    {
        var currentApplicationUserId = currentUserProvider.GetCurrentApplicationUserId();

        var trnRequest = await trnRequestHelper.GetTrnRequestInfo(currentApplicationUserId, command.RequestId);
        if (trnRequest is null)
        {
            return null;
        }

        var contact = (await crmQueryDispatcher.ExecuteQuery(
            new GetContactWithMergeResolutionQuery(
                trnRequest.ContactId,
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
                    Contact.Fields.MasterId))))!;

        var status = !string.IsNullOrEmpty(contact.dfeta_TRN) ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        // If we have metadata for the One Login user, ensure they're added to the OneLoginUsers table.
        // FUTURE: when TRN requests are handled exclusively in TRS this should be done at the point the task is resolved instead of here.
        if (status == TrnRequestStatus.Completed)
        {
            await trnRequestHelper.EnsureOneLoginUserIsConnected(trnRequest, contact);
        }

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId.ToString(),
            Person = new TrnRequestInfoPerson()
            {
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                MiddleName = contact.MiddleName,
                EmailAddress = contact.EMailAddress1,
                NationalInsuranceNumber = contact.dfeta_NINumber,
                DateOfBirth = contact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false)
            },
            Trn = contact.dfeta_TRN,
            Status = status,
        };
    }
}
