using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetTrnRequestCommand(string RequestId);

public class GetTrnRequestHandler(TrnRequestHelper trnRequestHelper, ICurrentUserProvider currentUserProvider)
{
    public async Task<TrnRequestInfo?> HandleAsync(GetTrnRequestCommand command)
    {
        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestHelper.GetTrnRequestInfoAsync(currentApplicationUserId, command.RequestId);
        if (trnRequest is null)
        {
            return null;
        }

        var contact = trnRequest.Contact;

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
            Status = trnRequest.IsCompleted ? TrnRequestStatus.Completed : TrnRequestStatus.Pending,
        };
    }
}
