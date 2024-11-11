using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetTrnRequestCommand(string RequestId);

public class GetTrnRequestHandler(TrnRequestHelper trnRequestHelper, ICurrentUserProvider currentUserProvider)
{
    public async Task<TrnRequestInfo?> Handle(GetTrnRequestCommand command)
    {
        var currentApplicationUserId = currentUserProvider.GetCurrentApplicationUserId();

        var trnRequest = await trnRequestHelper.GetTrnRequestInfo(currentApplicationUserId, command.RequestId);
        if (trnRequest is null)
        {
            return null;
        }

        var contact = trnRequest.Contact;

        // If we have metadata for the One Login user, ensure they're added to the OneLoginUsers table.
        // FUTURE: when TRN requests are handled exclusively in TRS this should be done at the point the task is resolved instead of here.
        if (trnRequest.Completed)
        {
            var metadata = await trnRequestHelper.GetRequestMetadata(trnRequest.ApplicationUserId, command.RequestId);

            if (metadata?.VerifiedOneLoginUserSubject is string oneLoginUserId)
            {
                await trnRequestHelper.EnsureOneLoginUserIsConnected(trnRequest, oneLoginUserId);
            }
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
            Status = trnRequest.Completed ? TrnRequestStatus.Completed : TrnRequestStatus.Pending,
        };
    }
}
