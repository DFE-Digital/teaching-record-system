using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetTrnRequestCommand(string RequestId);

public class GetTrnRequestHandler(
    TrsDbContext dbContext,
    TrnRequestHelper trnRequestHelper,
    ICurrentUserProvider currentUserProvider)
{
    public async Task<ApiResult<TrnRequestInfo>> HandleAsync(GetTrnRequestCommand command)
    {
        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestHelper.GetTrnRequestInfoAsync(currentApplicationUserId, command.RequestId);
        if (trnRequest is null)
        {
            return ApiError.TrnRequestDoesNotExist(command.RequestId);
        }

        // The request may have been completed since we last checked; ensure we have a TRN token if that's the case
        if (trnRequest.IsCompleted && trnRequest.TrnToken is null && trnRequest.Metadata.EmailAddress is string emailAddress)
        {
            var trnToken = await trnRequestHelper.CreateTrnTokenAsync(trnRequest.Trn, emailAddress);

            trnRequest.Metadata.TrnToken = trnToken;
            await dbContext.SaveChangesAsync();
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
                trnRequestHelper.GetAccessYourTeachingQualificationsLink(trnRequest.TrnToken) :
                null
        };
    }
}
