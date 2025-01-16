using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetTrnRequestCommand(string RequestId);

public class GetTrnRequestHandler(TrnRequestHelper trnRequestHelper, ICurrentUserProvider currentUserProvider)
{
    public async Task<ApiResult<TrnRequestInfo>> HandleAsync(GetTrnRequestCommand command)
    {
        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestHelper.GetTrnRequestInfoAsync(currentApplicationUserId, command.RequestId);
        if (trnRequest is null)
        {
            return ApiError.TrnRequestDoesNotExist(command.RequestId);
        }

        var contact = trnRequest.Contact;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
            Person = new TrnRequestInfoPerson()
            {
                // FUTURE - these values should be what was submitted in the original request
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                MiddleName = contact.MiddleName,
                EmailAddress = contact.EMailAddress1,
                NationalInsuranceNumber = contact.dfeta_NINumber,
                DateOfBirth = contact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false)
            },
            Trn = contact.dfeta_TRN,
            Status = trnRequest.IsCompleted ? TrnRequestStatus.Completed : TrnRequestStatus.Pending
        };
    }
}
