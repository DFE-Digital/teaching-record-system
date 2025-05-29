using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetTrnRequestCommand(string RequestId);

public class GetTrnRequestHandler(TrnRequestService trnRequestService, ICurrentUserProvider currentUserProvider)
{
    public async Task<ApiResult<TrnRequestInfo>> HandleAsync(GetTrnRequestCommand command)
    {
        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestService.GetTrnRequestInfoAsync(currentApplicationUserId, command.RequestId);
        if (trnRequest is null)
        {
            return ApiError.TrnRequestDoesNotExist(command.RequestId);
        }

        var contact = trnRequest.Contact;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
#pragma warning disable TRS0001
            Person = new TrnRequestInfoPerson()
#pragma warning restore TRS0001
            {
                // FUTURE - these values should be what was submitted in the original request
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                MiddleName = contact.MiddleName,
                EmailAddress = contact.EMailAddress1,
                NationalInsuranceNumber = contact.dfeta_NINumber,
                DateOfBirth = contact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false)
            },
            Trn = trnRequest.Trn,
            Status = trnRequest.IsCompleted ? TrnRequestStatus.Completed : TrnRequestStatus.Pending,
            PotentialDuplicate = trnRequest.PotentialDuplicate,
            AccessYourTeachingQualificationsLink = trnRequest.TrnToken is not null ?
                trnRequestService.GetAccessYourTeachingQualificationsLink(trnRequest.TrnToken) :
                null
        };
    }
}
